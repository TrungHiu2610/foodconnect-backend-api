using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Seller;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Seller.Queries
{
    public class GetSellerOrderStatisticsQueryHandler : IRequestHandler<GetSellerOrderStatisticsQuery, BaseResponse<SellerOrderStatisticsResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShopRepository _shopRepository;

        public GetSellerOrderStatisticsQueryHandler(
            IOrderRepository orderRepository,
            IShopRepository shopRepository)
        {
            _orderRepository = orderRepository;
            _shopRepository = shopRepository;
        }

        public async Task<BaseResponse<SellerOrderStatisticsResponse>> Handle(
            GetSellerOrderStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<SellerOrderStatisticsResponse>();

            var shop = await _shopRepository.GetByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            var ordersQuery = _orderRepository.GetAllQueryable()
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.ShopId == request.ShopId &&
                           o.CreatedAtUtc >= request.FromDate &&
                           o.CreatedAtUtc <= request.ToDate);

            if (request.Status.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Status == request.Status.Value);
            }

            var allOrders = await ordersQuery.ToListAsync(cancellationToken);

            var totalOrders = allOrders.Count;
            var completedOrders = allOrders.Count(o => o.Status == OrderStatusEnum.Completed);
            var cancelledOrders = allOrders.Count(o => o.Status == OrderStatusEnum.Cancelled);
            var pendingOrders = allOrders.Count(o => o.Status == OrderStatusEnum.Pending);

            var totalRevenue = allOrders
                .Where(o => o.Status == OrderStatusEnum.Completed)
                .Sum(o => (decimal)o.Total);

            var completionRate = totalOrders > 0
                ? Math.Round(((decimal)completedOrders / totalOrders) * 100, 2)
                : 0;

            var cancellationRate = totalOrders > 0
                ? Math.Round(((decimal)cancelledOrders / totalOrders) * 100, 2)
                : 0;

            var paginatedOrders = allOrders
                .OrderByDescending(o => o.CreatedAtUtc)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new OrderDetailItem
                {
                    OrderId = o.Id,
                    OrderCode = o.OrderCode,
                    CreatedAt = o.CreatedAtUtc,
                    Status = o.Status.ToString(),
                    Total = (decimal)o.Total,
                    BuyerName = o.Buyer.FullName,
                    ItemCount = o.OrderItems.Count,
                    Products = o.OrderItems.Select(oi => new OrderProductItem
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = (decimal)oi.UnitPrice,
                        TotalPrice = (decimal)oi.TotalPrice
                    }).ToList()
                })
                .ToList();

            var response = new SellerOrderStatisticsResponse
            {
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                PendingOrders = pendingOrders,
                TotalRevenue = totalRevenue,
                CompletionRate = completionRate,
                CancellationRate = cancellationRate,
                Orders = paginatedOrders,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            };

            return result.BuildSuccess(response, "Order statistics retrieved successfully");
        }
    }
}
