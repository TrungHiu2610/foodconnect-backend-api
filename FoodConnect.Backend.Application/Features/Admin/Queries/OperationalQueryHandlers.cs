using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using FoodConnect.Backend.Application.Commons.Helpers;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetOrderStatusOverviewQueryHandler : IRequestHandler<GetOrderStatusOverviewQuery, BaseResponse<OrderStatusOverviewResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRedisService _redisService;

        public GetOrderStatusOverviewQueryHandler(
            IOrderRepository orderRepository,
            IRedisService redisService)
        {
            _orderRepository = orderRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<OrderStatusOverviewResponse>> Handle(
            GetOrderStatusOverviewQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OrderStatusOverviewResponse>();

            var cacheKey = CacheKeys.OrderStatusOverview(request.FromDate, request.ToDate);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<OrderStatusOverviewResponse>>(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var orders = await _orderRepository.GetAllQueryable()
                .Where(o => o.CreatedAtUtc >= request.FromDate &&
                           o.CreatedAtUtc <= request.ToDate)
                .ToListAsync(cancellationToken);

            var totalOrders = orders.Count;
            var completedOrders = orders.Count(o => o.Status == OrderStatusEnum.Completed);
            var pendingOrders = orders.Count(o => o.Status == OrderStatusEnum.Pending);
            var cancelledOrders = orders.Count(o => o.Status == OrderStatusEnum.Cancelled);
            var refundedOrders = orders.Count(o => o.Status == OrderStatusEnum.Returned);

            var completionRate = totalOrders > 0
                ? Math.Round(((decimal)completedOrders / totalOrders) * 100, 2)
                : 0;

            var cancellationRate = totalOrders > 0
                ? Math.Round(((decimal)cancelledOrders / totalOrders) * 100, 2)
                : 0;

            var statusBreakdown = orders
                .GroupBy(o => o.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var response = new OrderStatusOverviewResponse
            {
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                PendingOrders = pendingOrders,
                CancelledOrders = cancelledOrders,
                RefundedOrders = refundedOrders,
                CompletionRate = completionRate,
                CancellationRate = cancellationRate,
                StatusBreakdown = statusBreakdown,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            };

            var successResult = result.BuildSuccess(response, "Order status overview retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.AdminOperational);
            return successResult;
        }
    }

    public class GetCancellationRateQueryHandler : IRequestHandler<GetCancellationRateQuery, BaseResponse<CancellationRateResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRedisService _redisService;

        public GetCancellationRateQueryHandler(
            IOrderRepository orderRepository,
            IRedisService redisService)
        {
            _orderRepository = orderRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<CancellationRateResponse>> Handle(
            GetCancellationRateQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CancellationRateResponse>();

            var cacheKey = CacheKeys.CancellationRate(request.FromDate, request.ToDate);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<CancellationRateResponse>>(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var currentOrders = await _orderRepository.GetAllQueryable()
                .Include(o => o.Shop)
                .Where(o => o.CreatedAtUtc >= request.FromDate &&
                           o.CreatedAtUtc <= request.ToDate)
                .ToListAsync(cancellationToken);

            var totalOrders = currentOrders.Count;
            var cancelledOrders = currentOrders.Count(o => o.Status == OrderStatusEnum.Cancelled);
            var overallCancellationRate = totalOrders > 0
                ? Math.Round(((decimal)cancelledOrders / totalOrders) * 100, 2)
                : 0;

            var cancellationsBySeller = currentOrders
                .GroupBy(o => new { o.ShopId, o.Shop.ShopName })
                .Select(g => new CancellationBySellerItem
                {
                    ShopId = g.Key.ShopId,
                    ShopName = g.Key.ShopName,
                    TotalOrders = g.Count(),
                    CancelledOrders = g.Count(o => o.Status == OrderStatusEnum.Cancelled),
                    CancellationRate = g.Count() > 0
                        ? Math.Round(((decimal)g.Count(o => o.Status == OrderStatusEnum.Cancelled) / g.Count()) * 100, 2)
                        : 0
                })
                .OrderByDescending(c => c.CancellationRate)
                .ToList();

            var previousMonthStart = request.FromDate.AddMonths(-1);
            var previousMonthEnd = request.FromDate.AddDays(-1);

            var previousOrders = await _orderRepository.GetAllQueryable()
                .Where(o => o.CreatedAtUtc >= previousMonthStart &&
                           o.CreatedAtUtc <= previousMonthEnd)
                .ToListAsync(cancellationToken);

            var previousCancelled = previousOrders.Count(o => o.Status == OrderStatusEnum.Cancelled);
            var previousRate = previousOrders.Count > 0
                ? ((decimal)previousCancelled / previousOrders.Count) * 100
                : 0;

            var response = new CancellationRateResponse
            {
                OverallCancellationRate = overallCancellationRate,
                TotalOrders = totalOrders,
                CancelledOrders = cancelledOrders,
                CancellationsBySeller = cancellationsBySeller,
                MonthComparison = new ComparisonData
                {
                    PreviousPeriodValue = previousRate,
                    CurrentPeriodValue = overallCancellationRate,
                    AbsoluteDelta = overallCancellationRate - previousRate,
                    PercentageChange = previousRate > 0
                        ? Math.Round(((overallCancellationRate - previousRate) / previousRate) * 100, 2)
                        : 0,
                    IsIncreased = overallCancellationRate >= previousRate
                }
            };

            var successResult = result.BuildSuccess(response, "Cancellation rate retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.AdminOperational);
            return successResult;
        }
    }

    public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, BaseResponse<TopProductsResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRedisService _redisService;

        public GetTopProductsQueryHandler(
            IOrderRepository orderRepository,
            ICategoryRepository categoryRepository,
            IRedisService redisService)
        {
            _orderRepository = orderRepository;
            _categoryRepository = categoryRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<TopProductsResponse>> Handle(
            GetTopProductsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<TopProductsResponse>();

            var cacheKey = CacheKeys.TopProducts(request.FromDate, request.ToDate, request.TopN, request.CategoryId);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<TopProductsResponse>>(cacheKey);

            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var ordersQuery = _orderRepository.GetAllQueryable()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductAssets)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Shop)
                .Where(o => o.Status == OrderStatusEnum.Completed);

            if (request.FromDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc <= request.ToDate.Value);
            }

            var orders = await ordersQuery.ToListAsync(cancellationToken);

            var productSales = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new
                {
                    oi.ProductId,
                    oi.Product.Name,
                    ShopName = oi.Product.Shop.ShopName,
                    CategoryName = oi.Product.Category.Name,
                    CategoryId = oi.Product.CategoryId,
                    Product = oi.Product
                })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.Name,
                    g.Key.ShopName,
                    g.Key.CategoryName,
                    g.Key.CategoryId,
                    g.Key.Product,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => (decimal)oi.TotalPrice),
                    OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .OrderByDescending(p => p.TotalRevenue)
                .ToList();

            var globalBestSellers = productSales
                .Take(request.TopN)
                .Select(p => new TopProductItem
                {
                    ProductId = p.ProductId,
                    ProductName = p.Name,
                    ProductImageUrl = p.Product?.ProductAssets?.FirstOrDefault()?.AssetUrl,
                    ShopName = p.ShopName,
                    CategoryName = p.CategoryName,
                    QuantitySold = p.QuantitySold,
                    TotalRevenue = p.TotalRevenue,
                    OrderCount = p.OrderCount
                })
                .ToList();

            var bestSellersByCategory = productSales
                .GroupBy(p => new { p.CategoryId, p.CategoryName })
                .Select(g => new CategoryBestSeller
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Products = g.OrderByDescending(p => p.TotalRevenue)
                        .Take(5)
                        .Select(p => new TopProductItem
                        {
                            ProductId = p.ProductId,
                            ProductName = p.Name,
                            ProductImageUrl = p.Product?.ProductAssets?.FirstOrDefault()?.AssetUrl,
                            ShopName = p.ShopName,
                            CategoryName = p.CategoryName,
                            QuantitySold = p.QuantitySold,
                            TotalRevenue = p.TotalRevenue,
                            OrderCount = p.OrderCount
                        })
                        .ToList()
                })
                .ToList();

            if (request.CategoryId.HasValue)
            {
                bestSellersByCategory = bestSellersByCategory
                    .Where(c => c.CategoryId == request.CategoryId.Value)
                    .ToList();
            }

            var response = new TopProductsResponse
            {
                GlobalBestSellers = globalBestSellers,
                BestSellersByCategory = bestSellersByCategory
            };

            var successResult = result.BuildSuccess(response, "Top products retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.AdminOperational);

            return successResult;
        }
    }
}
