using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Seller;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Seller.Queries
{
    public class GetSellerDashboardQueryHandler : IRequestHandler<GetSellerDashboardQuery, BaseResponse<SellerDashboardResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShopRepository _shopRepository;

        public GetSellerDashboardQueryHandler(
            IOrderRepository orderRepository,
            IShopRepository shopRepository)
        {
            _orderRepository = orderRepository;
            _shopRepository = shopRepository;
        }

        public async Task<BaseResponse<SellerDashboardResponse>> Handle(
            GetSellerDashboardQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<SellerDashboardResponse>();

            var shop = await _shopRepository.GetByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            var currentPeriodOrders = await _orderRepository.GetAllQueryable()
                .Include(o => o.OrderItems)
                .Where(o => o.ShopId == request.ShopId &&
                           o.CreatedAtUtc >= request.FromDate &&
                           o.CreatedAtUtc <= request.ToDate &&
                           o.Status == OrderStatusEnum.Completed)
                .ToListAsync(cancellationToken);

            var previousFromDate = CalculatePreviousPeriodStart(request.FromDate, request.Period);
            var previousToDate = CalculatePreviousPeriodEnd(request.ToDate, request.Period);

            var previousPeriodOrders = await _orderRepository.GetAllQueryable()
                .Include(o => o.OrderItems)
                .Where(o => o.ShopId == request.ShopId &&
                           o.CreatedAtUtc >= previousFromDate &&
                           o.CreatedAtUtc <= previousToDate &&
                           o.Status == OrderStatusEnum.Completed)
                .ToListAsync(cancellationToken);

            var currentRevenue = currentPeriodOrders.Sum(o => (decimal)o.Total);
            var previousRevenue = previousPeriodOrders.Sum(o => (decimal)o.Total);

            var currentOrdersCount = currentPeriodOrders.Count;
            var previousOrdersCount = previousPeriodOrders.Count;

            var currentProductsSold = currentPeriodOrders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity);
            var previousProductsSold = previousPeriodOrders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity);

            var response = new SellerDashboardResponse
            {
                TotalRevenue = currentRevenue,
                TotalOrders = currentOrdersCount,
                TotalProductsSold = currentProductsSold,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Period = request.Period,
                RevenueComparison = CalculateComparison(previousRevenue, currentRevenue),
                OrdersComparison = CalculateComparison(previousOrdersCount, currentOrdersCount),
                ProductsSoldComparison = CalculateComparison(previousProductsSold, currentProductsSold)
            };

            return result.BuildSuccess(response, "Dashboard data retrieved successfully");
        }

        private DateTime CalculatePreviousPeriodStart(DateTime fromDate, string period)
        {
            return period.ToLower() switch
            {
                "day" => fromDate.AddDays(-1),
                "month" => fromDate.AddMonths(-1),
                "year" => fromDate.AddYears(-1),
                _ => fromDate.AddMonths(-1)
            };
        }

        private DateTime CalculatePreviousPeriodEnd(DateTime toDate, string period)
        {
            return period.ToLower() switch
            {
                "day" => toDate.AddDays(-1),
                "month" => toDate.AddMonths(-1),
                "year" => toDate.AddYears(-1),
                _ => toDate.AddMonths(-1)
            };
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
}
