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
    public class ConfirmStandardDeliveryCommandHandler : IRequestHandler<ConfirmStandardDeliveryCommand, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;
        private readonly OrderNotificationService _orderNotificationService;

        public ConfirmStandardDeliveryCommandHandler(
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

        public async Task<BaseResponse<OrderDetailDto>> Handle(ConfirmStandardDeliveryCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OrderDetailDto>();
            string? uploadedImageUrl = null;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
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

                if (order.DeliveryType != DeliveryTypeEnum.Standard)
                {
                    return result.BuildFail("This endpoint is only for Standard delivery orders");
                }

                if (order.Status != OrderStatusEnum.OutForDelivery)
                {
                    return result.BuildFail($"Only OutForDelivery orders can be confirmed as delivered. Current status: {order.Status}");
                }

                if (string.IsNullOrEmpty(order.TrackingCode))
                {
                    return result.BuildFail("Tracking code must be set before confirming delivery");
                }

                uploadedImageUrl = await _fileStorageService.UploadFileAsync(
                    request.TrackingProofImage,
                    $"Orders/{order.OrderCode}/TrackingProof"
                );

                order.DeliveryProofImageUrl = uploadedImageUrl;
                order.Status = OrderStatusEnum.Delivered;
                order.DeliveredAt = DateTime.UtcNow;

                _orderRepository.Update(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
                var orderDto = OrderMapper.MapToDetailDto(order!);

                await _orderNotificationService.NotifyOrderDeliveredAsync(order!, cancellationToken);

                return result.BuildSuccess(orderDto, "Standard delivery confirmed successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (!string.IsNullOrEmpty(uploadedImageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(uploadedImageUrl);
                }

                return result.BuildFail($"Failed to confirm standard delivery: {ex.Message}");
            }
        }
    }
}
