using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class GetOrdersByShopQueryHandler : IRequestHandler<GetOrdersByShopQuery, BaseResponse<PaginatedList<OrderSummaryDto>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShopRepository _shopRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetOrdersByShopQueryHandler(
            IOrderRepository orderRepository,
            IShopRepository shopRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _shopRepository = shopRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<PaginatedList<OrderSummaryDto>>> Handle(GetOrdersByShopQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PaginatedList<OrderSummaryDto>>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var userId = _currentUserService.UserId.Value;

            // Check if shop belongs to user
            var shop = await _shopRepository.GetByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            if (shop.UserId != userId)
            {
                return result.BuildForbidden("You don't have permission to view orders for this shop");
            }

            var orders = await _orderRepository.GetOrdersByShopAsync(request.ShopId, request.Status);
            var orderDtos = orders.Select(o => OrderMapper.MapToSummaryDto(o)).ToList();

            // Apply pagination
            var totalCount = orderDtos.Count;
            var paginatedOrders = orderDtos
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var paginatedList = new PaginatedList<OrderSummaryDto>(
                paginatedOrders,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            return result.BuildSuccess(paginatedList, "Orders retrieved successfully");
        }
    }
}
