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
    public class ConfirmOrderReceivedCommandHandler : IRequestHandler<ConfirmOrderReceivedCommand, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;

        public ConfirmOrderReceivedCommandHandler(
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

        public async Task<BaseResponse<OrderDetailDto>> Handle(ConfirmOrderReceivedCommand request, CancellationToken cancellationToken)
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
                return result.BuildForbidden("You don't have permission to confirm this order");
            }

            // Check if order is delivered
            if (order.Status != OrderStatusEnum.Delivered)
            {
                return result.BuildFail("Only delivered orders can be confirmed as received");
            }

            // Update order status
            order.Status = OrderStatusEnum.Completed;
            order.CompletedAt = DateTime.UtcNow;

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload order with full details
            order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);

            // Send notification to seller
            await _orderNotificationService.NotifyOrderCompletedAsync(order!, cancellationToken);

            var orderDto = OrderMapper.MapToDetailDto(order!);

            return result.BuildSuccess(orderDto, "Order confirmed as received successfully");
        }
    }
}
