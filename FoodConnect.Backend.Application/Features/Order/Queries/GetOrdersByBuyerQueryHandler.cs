using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class GetOrdersByBuyerQueryHandler : IRequestHandler<GetOrdersByBuyerQuery, BaseResponse<List<OrderSummaryDto>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetOrdersByBuyerQueryHandler(
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<List<OrderSummaryDto>>> Handle(GetOrdersByBuyerQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<OrderSummaryDto>>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var buyerId = _currentUserService.UserId.Value;
            var orders = await _orderRepository.GetOrdersByBuyerAsync(buyerId, request.Status);

            var orderDtos = orders.Select(o => OrderMapper.MapToSummaryDto(o)).ToList();
            return result.BuildSuccess(orderDtos, "Orders retrieved successfully");
        }
    }
}
