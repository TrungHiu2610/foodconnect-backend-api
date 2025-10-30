using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;

        public CancelOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            OrderNotificationService orderNotificationService)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _orderNotificationService = orderNotificationService;
        }

        public async Task<BaseResponse<OrderDetailDto>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OrderDetailDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var buyerId = _currentUserService.UserId.Value;

            // Get order
            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            // Check if order belongs to buyer
            if (order.BuyerId != buyerId)
            {
                return result.BuildForbidden("You don't have permission to cancel this order");
            }

            // Check if order can be cancelled (only pending orders)
            if (order.Status != OrderStatusEnum.Pending)
            {
                return result.BuildFail("Only pending orders can be cancelled");
            }

            // Update order status
            order.Status = OrderStatusEnum.Cancelled;
            order.CancelReason = request.CancelReason;
            order.CancelledAt = DateTime.UtcNow;

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload order with full details
            order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            var orderDto = OrderMapper.MapToDetailDto(order!);

            // Send notification to seller (buyer cancelled)
            await _orderNotificationService.NotifyOrderCancelledAsync(order!, true, cancellationToken);

            return result.BuildSuccess(orderDto, "Order cancelled successfully");
        }
    }
}
