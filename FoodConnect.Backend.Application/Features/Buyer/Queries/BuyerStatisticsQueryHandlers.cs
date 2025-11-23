using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Buyer;
using FoodConnect.Backend.Application.Commons.Helpers;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FoodConnect.Backend.Application.Features.Buyer.Queries
{
    public class GetBuyerSpendingStatisticsQueryHandler : IRequestHandler<GetBuyerSpendingStatisticsQuery, BaseResponse<BuyerSpendingStatisticsResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRedisService _redisService;

        public GetBuyerSpendingStatisticsQueryHandler(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IRedisService redisService)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<BuyerSpendingStatisticsResponse>> Handle(
            GetBuyerSpendingStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<BuyerSpendingStatisticsResponse>();

            var buyer = await _userRepository.GetByIdAsync(request.BuyerId);
            if (buyer == null)
            {
                return result.BuildNotFound("Buyer not found");
            }

            // Check cache
            var cacheKey = CacheKeys.BuyerSpending(request.BuyerId, request.FromDate, request.ToDate);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<BuyerSpendingStatisticsResponse>>(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var ordersQuery = _orderRepository.GetAllQueryable()
                .Include(o => o.Shop)
                .Where(o => o.BuyerId == request.BuyerId &&
                           o.Status == OrderStatusEnum.Completed);

            if (request.FromDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc <= request.ToDate.Value);
            }

            var orders = await ordersQuery.ToListAsync(cancellationToken);

            var totalSpending = orders.Sum(o => (decimal)o.Total);
            var totalOrders = orders.Count;
            var avgOrderValue = totalOrders > 0 ? totalSpending / totalOrders : 0;

            var monthlySpending = orders
                .GroupBy(o => new { o.CreatedAtUtc.Year, o.CreatedAtUtc.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g =>
                {
                    var monthName = new DateTime(g.Key.Year, g.Key.Month, 1)
                        .ToString("MMMM yyyy", CultureInfo.InvariantCulture);
                    
                    return new MonthlySpendingItem
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = monthName,
                        TotalAmount = g.Sum(o => (decimal)o.Total),
                        OrderCount = g.Count()
                    };
                })
                .ToList();

            decimal cumulativeAmount = 0;
            foreach (var month in monthlySpending)
            {
                cumulativeAmount += month.TotalAmount;
                month.CumulativeAmount = cumulativeAmount;
            }

            var spendingByShop = orders
                .GroupBy(o => new { o.ShopId, o.Shop.ShopName })
                .Select(g => new SpendingByShopItem
                {
                    ShopId = g.Key.ShopId,
                    ShopName = g.Key.ShopName,
                    TotalSpent = g.Sum(o => (decimal)o.Total),
                    OrderCount = g.Count(),
                    PercentageOfTotal = totalSpending > 0
                        ? Math.Round((g.Sum(o => (decimal)o.Total) / totalSpending) * 100, 2)
                        : 0
                })
                .OrderByDescending(s => s.TotalSpent)
                .ToList();

            var response = new BuyerSpendingStatisticsResponse
            {
                TotalSpending = totalSpending,
                TotalOrders = totalOrders,
                AverageOrderValue = Math.Round(avgOrderValue, 2),
                MonthlySpending = monthlySpending,
                SpendingByShop = spendingByShop,
                FromDate = request.FromDate ?? orders.Min(o => o.CreatedAtUtc),
                ToDate = request.ToDate ?? orders.Max(o => o.CreatedAtUtc)
            };

            var successResult = result.BuildSuccess(response, "Buyer spending statistics retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.BuyerStats);
            return successResult;
        }
    }

    public class GetBuyerOrderActivityQueryHandler : IRequestHandler<GetBuyerOrderActivityQuery, BaseResponse<BuyerOrderActivityResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRedisService _redisService;

        public GetBuyerOrderActivityQueryHandler(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IRedisService redisService)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<BuyerOrderActivityResponse>> Handle(
            GetBuyerOrderActivityQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<BuyerOrderActivityResponse>();

            var buyer = await _userRepository.GetByIdAsync(request.BuyerId);
            if (buyer == null)
            {
                return result.BuildNotFound("Buyer not found");
            }

            // Check cache
            var cacheKey = CacheKeys.BuyerActivity(request.BuyerId, request.TopProductsCount);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<BuyerOrderActivityResponse>>(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var orders = await _orderRepository.GetAllQueryable()
                .Include(o => o.Shop)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductAssets)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .Where(o => o.BuyerId == request.BuyerId)
                .ToListAsync(cancellationToken);

            var totalOrders = orders.Count;
            var completedOrders = orders.Count(o => o.Status == OrderStatusEnum.Completed);
            var cancelledOrders = orders.Count(o => o.Status == OrderStatusEnum.Cancelled);
            var pendingOrders = orders.Count(o => o.Status == OrderStatusEnum.Pending);
            var refundedOrders = orders.Count(o => o.Status == OrderStatusEnum.Returned);

            var completionRate = totalOrders > 0
                ? Math.Round(((decimal)completedOrders / totalOrders) * 100, 2)
                : 0;

            var orderStatusBreakdown = orders
                .GroupBy(o => o.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var completedOrdersForProducts = orders
                .Where(o => o.Status == OrderStatusEnum.Completed)
                .ToList();

            var topPurchasedProducts = completedOrdersForProducts
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new
                {
                    oi.ProductId,
                    oi.Product.Name,
                    ShopName = oi.Product.Shop.ShopName,
                    CategoryName = oi.Product.Category.Name,
                    Product = oi.Product
                })
                .Select(g => new TopPurchasedProductItem
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    ProductImageUrl = g.Key.Product?.ProductAssets?.FirstOrDefault()?.AssetUrl,
                    ShopName = g.Key.ShopName,
                    CategoryName = g.Key.CategoryName,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalSpent = g.Sum(oi => (decimal)oi.TotalPrice),
                    OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .OrderByDescending(p => p.TotalQuantity)
                .Take(request.TopProductsCount)
                .ToList();

            var interactedSellers = completedOrdersForProducts
                .GroupBy(o => new { o.ShopId, o.Shop.ShopName })
                .Select(g => new InteractedSellerItem
                {
                    ShopId = g.Key.ShopId,
                    ShopName = g.Key.ShopName,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(o => (decimal)o.Total),
                    FirstOrderDate = g.Min(o => o.CreatedAtUtc),
                    LastOrderDate = g.Max(o => o.CreatedAtUtc)
                })
                .OrderByDescending(s => s.OrderCount)
                .ToList();

            var response = new BuyerOrderActivityResponse
            {
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                PendingOrders = pendingOrders,
                RefundedOrders = refundedOrders,
                CompletionRate = completionRate,
                OrderStatusBreakdown = orderStatusBreakdown,
                TopPurchasedProducts = topPurchasedProducts,
                InteractedSellers = interactedSellers,
                FirstOrderDate = orders.Any() ? orders.Min(o => o.CreatedAtUtc) : null,
                LastOrderDate = orders.Any() ? orders.Max(o => o.CreatedAtUtc) : null
            };

            var successResult = result.BuildSuccess(response, "Buyer order activity retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.BuyerStats);
            return successResult;
        }
    }
}
