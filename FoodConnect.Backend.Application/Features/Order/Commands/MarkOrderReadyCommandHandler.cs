using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class MarkOrderReadyCommandHandler : IRequestHandler<MarkOrderReadyCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public MarkOrderReadyCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(MarkOrderReadyCommand request, CancellationToken cancellationToken)
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

            if (order.Status != OrderStatusEnum.Prepared && order.Status != OrderStatusEnum.Preparing)
            {
                return result.BuildFail($"Cannot mark order as ready. Current status is {order.Status}");
            }

            order.Status = OrderStatusEnum.ReadyForPickup;
            order.ReadyForPickupAt = DateTime.UtcNow;

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return result.BuildSuccess(
                new CreateOrUpdateResponse { Id = order.Id },
                "Order marked as ready for pickup"
            );
        }
    }
}
