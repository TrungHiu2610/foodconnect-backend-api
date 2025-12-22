using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Application.Features.Order.Jobs
{
    public class ExpressDeliveryTimeoutJob
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly OrderNotificationService _notificationService;
        private readonly ILogger<ExpressDeliveryTimeoutJob> _logger;

        public ExpressDeliveryTimeoutJob(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork,
            OrderNotificationService notificationService,
            ILogger<ExpressDeliveryTimeoutJob> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task CheckAndCancelExpiredOrderAsync(Guid orderId)
        {
            try
            {
                _logger.LogInformation("Checking express order {OrderId} for timeout at {Time}", orderId, DateTime.UtcNow);

                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                
                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", orderId);
                    return;
                }

                if (order.DeliveryType != DeliveryTypeEnum.Express && order.DeliveryType != DeliveryTypeEnum.Standard)
                {
                    _logger.LogWarning("Order {OrderId} is not Express or Standard delivery, skipping", orderId);
                    return;
                }

                if (order.Status == OrderStatusEnum.Cancelled)
                {
                    _logger.LogInformation("Order {OrderId} already cancelled", orderId);
                    return;
                }

                var now = DateTime.UtcNow;
                var timeSinceCreated = now - order.CreatedAtUtc;

                if (order.DeliveryType == DeliveryTypeEnum.Express)
                {
                    if (order.Status == OrderStatusEnum.Pending)
                    {
                        if (timeSinceCreated >= TimeSpan.FromMinutes(30))
                        {
                            await CancelOrderAsync(order, "Đơn hàng Express tự động hủy do người bán không xác nhận sau 30 phút");
                        }
                        else
                        {
                            _logger.LogInformation("Order {OrderId} not yet timed out (Pending, {Minutes} minutes elapsed)", orderId, timeSinceCreated.TotalMinutes);
                        }
                    }
                    else if (order.Status == OrderStatusEnum.Preparing)
                    {
                        if (timeSinceCreated >= TimeSpan.FromHours(3))
                        {
                            await CancelOrderAsync(order, "Đơn hàng Express tự động hủy do không giao trong vòng 3 giờ");
                        }
                        else
                        {
                            _logger.LogInformation("Order {OrderId} not yet timed out ({Status}, {Hours} hours elapsed)", orderId, order.Status, timeSinceCreated.TotalHours);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Order {OrderId} status is {Status}, no auto-cancel needed", orderId, order.Status);
                    }
                }
                else if (order.DeliveryType == DeliveryTypeEnum.Standard)
                {
                    if (order.Status == OrderStatusEnum.Pending)
                    {
                        if (timeSinceCreated >= TimeSpan.FromHours(3))
                        {
                            await CancelOrderAsync(order, "Đơn hàng Standard tự động hủy do người bán không xác nhận sau 3 giờ");
                        }
                        else
                        {
                            _logger.LogInformation("Order {OrderId} not yet timed out (Standard Pending, {Hours} hours elapsed)", orderId, timeSinceCreated.TotalHours);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Order {OrderId} status is {Status}, no auto-cancel needed for Standard", orderId, order.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking timeout for order {OrderId}", orderId);
            }
        }

        private async Task CancelOrderAsync(Domain.Entities.Order order, string cancelReason)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (order.Status == OrderStatusEnum.Preparing)
                {
                    foreach (var orderItem in order.OrderItems)
                    {
                        var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                        
                        if (product != null && product.StockQuantity.HasValue)
                        {
                            product.StockQuantity += orderItem.Quantity;
                            if (product.StockQuantity > 0)
                            {
                                product.IsAvailable = true;
                            }
                            _productRepository.Update(product);
                        }
                    }
                }

                if (order.PaymentMethod == PaymentMethodEnum.COD && order.Status == OrderStatusEnum.Preparing)
                {
                    var sellerId = order.Shop.UserId;
                    var sellerWallet = await _walletRepository.GetByUserIdAndTypeAsync(sellerId, WalletTypeEnum.Seller);
                    
                    if (sellerWallet != null)
                    {
                        var orderTotal = (decimal)order.Total;
                        sellerWallet.PendingBalance -= orderTotal;
                        _walletRepository.Update(sellerWallet);
                        
                        _logger.LogInformation("Released pending balance {Amount} for seller {SellerId}", orderTotal, sellerId);
                    }
                }

                order.Status = OrderStatusEnum.Cancelled;
                order.CancelReason = cancelReason;
                order.CancelledAt = DateTime.UtcNow;
                _orderRepository.Update(order);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Auto-cancelled order {OrderCode}: {Reason}", order.OrderCode, cancelReason);

                try
                {
                    await _notificationService.NotifyOrderCancelledAsync(order, false, CancellationToken.None);
                }
                catch (Exception notifyEx)
                {
                    _logger.LogError(notifyEx, "Failed to send cancellation notification for order {OrderId}", order.Id);
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to cancel order {OrderId}", order.Id);
                throw;
            }
        }
    }
}
