using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetOrderDetailQueryHandler(
            IOrderRepository _orderRepository,
            ICurrentUserService currentUserService)
        {
            this._orderRepository = _orderRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<OrderDetailDto>> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OrderDetailDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            // Check if user has permission to view this order
            var userId = _currentUserService.UserId.Value;
            var isBuyer = order.BuyerId == userId;
            var isSeller = order.Shop?.UserId == userId;

            if (!isBuyer && !isSeller)
            {
                return result.BuildForbidden("You don't have permission to view this order");
            }

            var orderDto = OrderMapper.MapToDetailDto(order);
            return result.BuildSuccess(orderDto, "Order retrieved successfully");
        }
    }
}
