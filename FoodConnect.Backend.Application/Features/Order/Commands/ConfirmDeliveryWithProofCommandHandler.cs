using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class ConfirmDeliveryWithProofCommandHandler : IRequestHandler<ConfirmDeliveryWithProofCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;
        private readonly OrderNotificationService _orderNotificationService;

        public ConfirmDeliveryWithProofCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IFileStorageService fileStorageService,
            OrderNotificationService orderNotificationService)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
            _orderNotificationService = orderNotificationService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(ConfirmDeliveryWithProofCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();
            string? uploadedImageUrl = null;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
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

                if (order.Status != OrderStatusEnum.DeliveryingBySeller)
                {
                    return result.BuildFail($"Order must be in DeliveryingBySeller status. Current status is {order.Status}");
                }

                uploadedImageUrl = await _fileStorageService.UploadFileAsync(
                    request.DeliveryProofImage,
                    $"Orders/{order.OrderCode}/DeliveryProof"
                );

                order.DeliveryProofImageUrl = uploadedImageUrl;
                order.Status = OrderStatusEnum.Delivered;
                order.DeliveredAt = DateTime.UtcNow;

                _orderRepository.Update(order);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync(transaction);

                // Reload order with full details for notification
                order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
                await _orderNotificationService.NotifyOrderDeliveredAsync(order!, cancellationToken);

                return result.BuildSuccess(
                    new CreateOrUpdateResponse { Id = order.Id },
                    "Order marked as delivered. Waiting for buyer confirmation."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (!string.IsNullOrEmpty(uploadedImageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(uploadedImageUrl);
                }

                return result.BuildFail($"Failed to confirm delivery: {ex.Message}");
            }
        }
    }
}
