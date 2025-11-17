using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class RequestShipperCommandHandler : IRequestHandler<RequestShipperCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RequestShipperCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(RequestShipperCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            // Check authorization
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return result.BuildUnauthorized();
            }

            // Get order with shop info
            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            // Verify seller owns this order's shop
            if (order.Shop.UserId != userId.Value)
            {
                return result.BuildForbidden("You don't have permission to update this order");
            }

            // Validate this is an Express order
            if (order.DeliveryType != DeliveryTypeEnum.Express)
            {
                return result.BuildFail("Shipper request is only available for Express orders");
            }

            // Validate status transition
            if (order.Status != OrderStatusEnum.ReadyForPickup)
            {
                return result.BuildFail($"Order must be in ReadyForPickup status. Current status is {order.Status}");
            }

            // Update order status to finding shipper
            order.Status = OrderStatusEnum.DeliveryingByShipper;
            order.DeliveryStartedAt = DateTime.UtcNow;

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            // TODO: Integrate with shipper finding system (future implementation)
            // - Call external API to find available shippers
            // - Send notification to nearby shippers
            // - Create shipper assignment record

            return result.BuildSuccess(
                new CreateOrUpdateResponse { Id = order.Id },
                "Finding shipper for your order. You will be notified when a shipper accepts."
            );
        }
    }
}
