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
    public class RejectOrderCommandHandler : IRequestHandler<RejectOrderCommand, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;

        public RejectOrderCommandHandler(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            OrderNotificationService orderNotificationService,
            INotificationRepository notificationRepository,
            INotificationService notificationService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _orderNotificationService = orderNotificationService;
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<OrderDetailDto>> Handle(RejectOrderCommand request, CancellationToken cancellationToken)
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
                return result.BuildForbidden("You don't have permission to reject this order");
            }

            if (order.Status != OrderStatusEnum.Pending)
            {
                return result.BuildFail("Only pending orders can be rejected");
            }

            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return result.BuildFail("Rejection reason is required");
            }

            // Restore stock for rejected orders (stock was deducted during CreateOrder)
            foreach (var orderItem in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                if (product != null && product.StockQuantity.HasValue)
                {
                    product.StockQuantity += orderItem.Quantity;
                    
                    // Make product available again if it was out of stock
                    if (product.StockQuantity > 0 && !product.IsAvailable)
                    {
                        product.IsAvailable = true;
                    }
                    
                    _productRepository.Update(product);
                }
            }

            order.Status = OrderStatusEnum.Rejected;
            order.CancelReason = request.RejectionReason;
            order.CancelledAt = DateTime.UtcNow;

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            var orderDto = OrderMapper.MapToDetailDto(order!);

            await _orderNotificationService.NotifyOrderRejectedAsync(order!, cancellationToken);
            
            var newOrderNotification = await _notificationRepository
                .GetNotificationByOrderIdAsync(order!.Id, order.Shop!.UserId, Domain.Enums.NotificationTypeEnum.NewOrder);
            
            if (newOrderNotification != null)
            {
                await _notificationService.StopSoundAlertAsync(order.Shop.UserId, newOrderNotification.Id);
            }

            return result.BuildSuccess(orderDto, "Order rejected successfully");
        }
    }
}
