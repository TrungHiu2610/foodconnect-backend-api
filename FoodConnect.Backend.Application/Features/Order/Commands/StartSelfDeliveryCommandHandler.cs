using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class StartSelfDeliveryCommandHandler : IRequestHandler<StartSelfDeliveryCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;

        public StartSelfDeliveryCommandHandler(
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

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(StartSelfDeliveryCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return result.BuildUnauthorized();
            }

            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            if (order.Shop.UserId != userId.Value)
            {
                return result.BuildForbidden("You don't have permission to update this order");
            }

            if (order.DeliveryType != DeliveryTypeEnum.Express)
            {
                return result.BuildFail("Self-delivery is only available for Express orders");
            }

            if (order.Status != OrderStatusEnum.ReadyForPickup)
            {
                return result.BuildFail($"Order must be in ReadyForPickup status. Current status is {order.Status}");
            }

            order.Status = OrderStatusEnum.DeliveryingBySeller;
            order.DeliveryStartedAt = DateTime.UtcNow;

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            // Reload order with full details for notification
            order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            await _orderNotificationService.NotifyOrderOutForDeliveryAsync(order!, cancellationToken);

            return result.BuildSuccess(
                new CreateOrUpdateResponse { Id = order.Id },
                "Started self-delivery. Please upload delivery proof photo when arrived."
            );
        }
    }
}
