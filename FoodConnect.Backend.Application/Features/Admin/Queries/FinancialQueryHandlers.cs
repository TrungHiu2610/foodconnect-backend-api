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
    public class GetSystemRevenueQueryHandler : IRequestHandler<GetSystemRevenueQuery, BaseResponse<SystemRevenueResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly IRedisService _redisService;

        public GetSystemRevenueQueryHandler(
            IOrderRepository orderRepository,
            ISystemConfigRepository systemConfigRepository,
            IRedisService redisService)
        {
            _orderRepository = orderRepository;
            _systemConfigRepository = systemConfigRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<SystemRevenueResponse>> Handle(
            GetSystemRevenueQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<SystemRevenueResponse>();

            var cacheKey = CacheKeys.SystemRevenue(request.FromDate, request.ToDate, request.SellerId);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<SystemRevenueResponse>>(cacheKey);

            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var platformFeeConfig = await _systemConfigRepository.GetByKeyAsync("PLATFORM_FEE_PERCENTAGE");
            var platformFeePercentage = platformFeeConfig != null
                ? decimal.Parse(platformFeeConfig.ConfigValue)
                : 10m;

            var currentOrders = _orderRepository.GetAllQueryable()
                .Where(o => o.Status == OrderStatusEnum.Completed &&
                           o.CreatedAtUtc >= request.FromDate &&
                           o.CreatedAtUtc <= request.ToDate);

            if (request.SellerId.HasValue)
            {
                currentOrders = currentOrders.Where(o => o.Shop.UserId == request.SellerId.Value);
            }

            var currentOrdersList = await currentOrders.ToListAsync(cancellationToken);
            var currentGMV = currentOrdersList.Sum(o => (decimal)o.Total);
            var currentRevenue = currentGMV * (platformFeePercentage / 100);
            var currentCount = currentOrdersList.Count;

            var periodDays = (request.ToDate - request.FromDate).Days;
            var previousFromDate = request.FromDate.AddDays(-periodDays);
            var previousToDate = request.FromDate.AddDays(-1);

            var previousOrders = await _orderRepository.GetAllQueryable()
                .Where(o => o.Status == OrderStatusEnum.Completed &&
                           o.CreatedAtUtc >= previousFromDate &&
                           o.CreatedAtUtc <= previousToDate)
                .ToListAsync(cancellationToken);

            var previousGMV = previousOrders.Sum(o => (decimal)o.Total);
            var previousRevenue = previousGMV * (platformFeePercentage / 100);

            var response = new SystemRevenueResponse
            {
                GMV = currentGMV,
                PlatformRevenue = currentRevenue,
                PlatformFeePercentage = platformFeePercentage,
                TotalCompletedOrders = currentCount,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                GMVComparison = CalculateComparison(previousGMV, currentGMV),
                RevenueComparison = CalculateComparison(previousRevenue, currentRevenue)
            };

            var successResult = result.BuildSuccess(response, "System revenue retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.AdminFinancial);

            return successResult;
        }

        private ComparisonData CalculateComparison(decimal previousValue, decimal currentValue)
        {
            var delta = currentValue - previousValue;
            var percentageChange = previousValue == 0
                ? (currentValue > 0 ? 100 : 0)
                : (delta / previousValue) * 100;

            return new ComparisonData
            {
                PreviousPeriodValue = previousValue,
                CurrentPeriodValue = currentValue,
                AbsoluteDelta = delta,
                PercentageChange = Math.Round(percentageChange, 2),
                IsIncreased = delta >= 0
            };
        }
    }

    public class GetRevenueBySellerQueryHandler : IRequestHandler<GetRevenueBySellerQuery, BaseResponse<RevenueBySellerResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly IRedisService _redisService;

        public GetRevenueBySellerQueryHandler(
            IOrderRepository orderRepository,
            ISystemConfigRepository systemConfigRepository,
            IRedisService redisService)
        {
            _orderRepository = orderRepository;
            _systemConfigRepository = systemConfigRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<RevenueBySellerResponse>> Handle(
            GetRevenueBySellerQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<RevenueBySellerResponse>();

            // Check cache
            var cacheKey = CacheKeys.RevenueBySeller(request.FromDate, request.ToDate, request.TopN);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<RevenueBySellerResponse>>(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var platformFeeConfig = await _systemConfigRepository.GetByKeyAsync("PLATFORM_FEE_PERCENTAGE");
            var platformFeePercentage = platformFeeConfig != null
                ? decimal.Parse(platformFeeConfig.ConfigValue)
                : 10m;

            var orders = await _orderRepository.GetAllQueryable()
                .Include(o => o.Shop)
                    .ThenInclude(s => s.User)
                .Where(o => o.Status == OrderStatusEnum.Completed &&
                           o.CreatedAtUtc >= request.FromDate &&
                           o.CreatedAtUtc <= request.ToDate)
                .ToListAsync(cancellationToken);

            var sellerRevenues = orders
                .GroupBy(o => new { o.ShopId, o.Shop.ShopName, o.Shop.UserId, SellerName = o.Shop.User.FullName })
                .Select(g => new SellerRevenueItem
                {
                    SellerId = g.Key.UserId,
                    ShopId = g.Key.ShopId,
                    ShopName = g.Key.ShopName,
                    SellerName = g.Key.SellerName,
                    TotalRevenue = g.Sum(o => (decimal)o.Total),
                    PlatformFee = g.Sum(o => (decimal)o.Total) * (platformFeePercentage / 100),
                    TotalOrders = g.Count(),
                    AverageOrderValue = g.Any() ? g.Average(o => (decimal)o.Total) : 0
                })
                .OrderByDescending(s => s.TotalRevenue)
                .Take(request.TopN)
                .ToList();

            var response = new RevenueBySellerResponse
            {
                SellerRevenues = sellerRevenues,
                TotalGMV = sellerRevenues.Sum(s => s.TotalRevenue),
                TotalPlatformFee = sellerRevenues.Sum(s => s.PlatformFee)
            };

            var successResult = result.BuildSuccess(response, "Revenue by seller retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.AdminFinancial);
            return successResult;
        }
    }

    public class GetRefundStatisticsQueryHandler : IRequestHandler<GetRefundStatisticsQuery, BaseResponse<RefundStatisticsResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRedisService _redisService;

        public GetRefundStatisticsQueryHandler(
            IOrderRepository orderRepository,
            IRedisService redisService)
        {
            _orderRepository = orderRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<RefundStatisticsResponse>> Handle(
            GetRefundStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<RefundStatisticsResponse>();

            // Check cache
            var cacheKey = CacheKeys.RefundStats(request.FromDate, request.ToDate, request.SellerId);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<RefundStatisticsResponse>>(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var ordersQuery = _orderRepository.GetAllQueryable()
                .Include(o => o.Shop)
                .Where(o => o.CreatedAtUtc >= request.FromDate &&
                           o.CreatedAtUtc <= request.ToDate);

            if (request.SellerId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Shop.UserId == request.SellerId.Value);
            }

            var orders = await ordersQuery.ToListAsync(cancellationToken);

            var refundedOrders = orders.Where(o => o.Status == OrderStatusEnum.Returned).ToList();
            var completedOrders = orders.Where(o => o.Status == OrderStatusEnum.Completed).ToList();

            var totalRefundAmount = refundedOrders.Sum(o => (decimal)o.Total);
            var totalRevenue = completedOrders.Sum(o => (decimal)o.Total);
            var refundRate = (totalRevenue + totalRefundAmount) > 0
                ? Math.Round((totalRefundAmount / (totalRevenue + totalRefundAmount)) * 100, 2)
                : 0;

            var refundsBySeller = refundedOrders
                .GroupBy(o => new { o.ShopId, o.Shop.ShopName })
                .Select(g => new RefundBySellerItem
                {
                    ShopId = g.Key.ShopId,
                    ShopName = g.Key.ShopName,
                    RefundAmount = g.Sum(o => (decimal)o.Total),
                    RefundCount = g.Count(),
                    RefundRate = Math.Round((g.Sum(o => (decimal)o.Total) / totalRefundAmount) * 100, 2)
                })
                .OrderByDescending(r => r.RefundAmount)
                .ToList();

            var response = new RefundStatisticsResponse
            {
                TotalRefundAmount = totalRefundAmount,
                TotalRefundCount = refundedOrders.Count,
                RefundRate = refundRate,
                TotalRevenue = totalRevenue,
                RefundsBySeller = refundsBySeller,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            };

            var successResult = result.BuildSuccess(response, "Refund statistics retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.AdminFinancial);
            return successResult;
        }
    }
}
