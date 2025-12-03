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
    public class MarkAsPreparedCommandHandler : IRequestHandler<MarkAsPreparedCommand, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;

        public MarkAsPreparedCommandHandler(
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

        public async Task<BaseResponse<OrderDetailDto>> Handle(MarkAsPreparedCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OrderDetailDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var userId = _currentUserService.UserId.Value;

            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            if (order.Shop?.UserId != userId)
            {
                return result.BuildForbidden("You don't have permission to update this order");
            }

            if (order.Status != OrderStatusEnum.Preparing)
            {
                return result.BuildFail("Only preparing orders can be marked as prepared");
            }

            if (order.DeliveryType == DeliveryTypeEnum.Express)
            {
                order.Status = OrderStatusEnum.ReadyForPickup;
                order.ReadyForPickupAt = DateTime.UtcNow;
            }
            else
            {
                order.Status = OrderStatusEnum.Prepared;
                order.PreparedAt = DateTime.UtcNow;
            }

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            var orderDto = OrderMapper.MapToDetailDto(order!);

            await _orderNotificationService.NotifyOrderPreparingAsync(order!, cancellationToken);

            return result.BuildSuccess(orderDto, "Order marked as prepared and ready for delivery");
        }
    }
}
