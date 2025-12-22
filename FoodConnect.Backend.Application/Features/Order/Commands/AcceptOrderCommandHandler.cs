using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Jobs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using Hangfire;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class AcceptOrderCommandHandler : IRequestHandler<AcceptOrderCommand, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;

        public AcceptOrderCommandHandler(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            OrderNotificationService orderNotificationService,
            INotificationRepository notificationRepository,
            INotificationService notificationService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _walletRepository = walletRepository;
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

            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            if (order.Shop?.UserId != userId)
            {
                return result.BuildForbidden("You don't have permission to accept this order");
            }

            if (order.Status != OrderStatusEnum.Pending)
            {
                if (order.Status == OrderStatusEnum.AwaitingPayment)
                {
                    return result.BuildFail("Order is awaiting payment. Customer must complete payment first.");
                }
                return result.BuildFail("Only pending orders can be accepted");
            }

            if (order.PaymentMethod == PaymentMethodEnum.COD)
            {
                var sellerWallet = await _walletRepository.GetByUserIdAndTypeAsync(userId, WalletTypeEnum.Seller);
                
                if (sellerWallet == null)
                {
                    sellerWallet = new Domain.Entities.Wallet
                    {
                        UserId = userId,
                        WalletType = WalletTypeEnum.Seller,
                        Balance = 0,
                        TotalEarned = 0,
                        TotalWithdrawn = 0,
                        PendingBalance = 0,
                        Status = WalletStatusEnum.Active
                    };
                    await _walletRepository.AddAsync(sellerWallet);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                var availableBalance = sellerWallet.Balance - sellerWallet.PendingBalance;
                var orderTotal = (decimal)order.Total;

                if (availableBalance < orderTotal)
                {
                    return result.BuildFail(
                        $"Không đủ số dư trong ví để chấp nhận đơn COD. " +
                        $"Cần: {orderTotal:N0} VNĐ, Có: {availableBalance:N0} VNĐ. " +
                        $"Vui lòng nạp thêm tiền vào ví trước khi chấp nhận đơn hàng."
                    );
                }

                sellerWallet.PendingBalance += orderTotal;
                _walletRepository.Update(sellerWallet);
            }

            foreach (var orderItem in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                if (product == null)
                {
                    return result.BuildFail($"Product {orderItem.ProductId} not found");
                }

                if (product.StockQuantity.HasValue)
                {
                    if (product.StockQuantity.Value < orderItem.Quantity)
                    {
                        return result.BuildFail($"Insufficient stock for product: {product.Name}. Available: {product.StockQuantity}, Required: {orderItem.Quantity}");
                    }

                    product.StockQuantity -= orderItem.Quantity;
                    
                    if (product.StockQuantity <= 0)
                    {
                        product.IsAvailable = false;
                    }

                    _productRepository.Update(product);
                }
            }

            order.Status = OrderStatusEnum.Preparing;
            order.AcceptedAt = DateTime.UtcNow;

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (order.DeliveryType == DeliveryTypeEnum.Express)
            {
                var timeSinceCreated = DateTime.UtcNow - order.CreatedAtUtc;
                var remainingTime = TimeSpan.FromHours(3) - timeSinceCreated;
                
                if (remainingTime > TimeSpan.Zero)
                {                    
                    BackgroundJob.Schedule<ExpressDeliveryTimeoutJob>(
                        job => job.CheckAndCancelExpiredOrderAsync(order.Id),
                        remainingTime);
                }
            }

            order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            var orderDto = OrderMapper.MapToDetailDto(order!);

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
