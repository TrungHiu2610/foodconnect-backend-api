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
    public class AcceptOrderCommandHandler : IRequestHandler<AcceptOrderCommand, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;

        public AcceptOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            OrderNotificationService orderNotificationService,
            INotificationRepository notificationRepository,
            INotificationService notificationService)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _orderNotificationService = orderNotificationService;
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<OrderDetailDto>> Handle(AcceptOrderCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OrderDetailDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var userId = _currentUserService.UserId.Value;

            // Get order
            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            // Check if order belongs to seller's shop
            if (order.Shop?.UserId != userId)
            {
                return result.BuildForbidden("You don't have permission to accept this order");
            }

            // Check if order is pending
            if (order.Status != OrderStatusEnum.Pending)
            {
                return result.BuildFail("Only pending orders can be accepted");
            }

            // Update order status
            order.Status = OrderStatusEnum.Preparing;
            order.AcceptedAt = DateTime.UtcNow;

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload order with full details
            order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            var orderDto = OrderMapper.MapToDetailDto(order!);

            // Send notification to buyer
            await _orderNotificationService.NotifyOrderAcceptedAsync(order!, cancellationToken);
            
            var newOrderNotification = await _notificationRepository
                .GetNotificationByOrderIdAsync(order!.Id, order.Shop!.UserId, Domain.Enums.NotificationTypeEnum.NewOrder);
            
            if (newOrderNotification != null)
            {
                await _notificationService.StopSoundAlertAsync(order.Shop.UserId, newOrderNotification.Id);
            }

            return result.BuildSuccess(orderDto, "Order accepted successfully");
        }
    }
}
