using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Application.Features.Order.Services
{
    public class OrderAutoCompletionService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ISellerWalletRepository _walletRepository;
        private readonly ISellerWalletTransactionRepository _transactionRepository;
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly OrderNotificationService _orderNotificationService;
        private readonly ILogger<OrderAutoCompletionService> _logger;

        public OrderAutoCompletionService(
            IOrderRepository orderRepository,
            ISellerWalletRepository walletRepository,
            ISellerWalletTransactionRepository transactionRepository,
            ISystemConfigRepository systemConfigRepository,
            IUnitOfWork unitOfWork,
            OrderNotificationService orderNotificationService,
            ILogger<OrderAutoCompletionService> logger)
        {
            _orderRepository = orderRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _systemConfigRepository = systemConfigRepository;
            _unitOfWork = unitOfWork;
            _orderNotificationService = orderNotificationService;
            _logger = logger;
        }

        public async Task<bool> CompleteOrderAsync(Domain.Entities.Order order, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate order status
                if (order.Status != OrderStatusEnum.Delivered)
                {
                    _logger.LogWarning("Order {OrderId} is not in Delivered status, cannot complete", order.Id);
                    return false;
                }

                // Check payment status
                if (order.PaymentMethod != PaymentMethodEnum.COD && order.PaymentStatus != PaymentStatusEnum.Paid)
                {
                    _logger.LogWarning("Order {OrderId} is not paid, cannot complete", order.Id);
                    return false;
                }

                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Update order status
                    order.Status = OrderStatusEnum.Completed;
                    order.CompletedAt = DateTime.UtcNow;
                    _orderRepository.Update(order);

                    // Get or create seller wallet
                    var sellerId = order.Shop.UserId;
                    var wallet = await _walletRepository.GetBySellerIdAsync(sellerId);

                    if (wallet == null)
                    {
                        wallet = new SellerWallet
                        {
                            SellerId = sellerId,
                            Balance = 0,
                            TotalEarned = 0,
                            TotalWithdrawn = 0,
                            PendingBalance = 0,
                            Status = SellerWalletStatusEnum.Active
                        };
                        await _walletRepository.AddAsync(wallet);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    // Calculate commission and earnings
                    var commissionRate = await _systemConfigRepository.GetCommissionRateAsync();
                    var commissionableAmount = (decimal)(order.Total - order.ShippingFee);
                    var commissionAmount = commissionableAmount * (commissionRate / 100);
                    var sellerEarning = commissionableAmount - commissionAmount;

                    var balanceBefore = wallet.Balance;

                    // Create earning transaction
                    var earningTransaction = new SellerWalletTransaction
                    {
                        WalletId = wallet.Id,
                        OrderId = order.Id,
                        TransactionType = TransactionTypeEnum.OrderEarning,
                        Amount = commissionableAmount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceBefore + commissionableAmount,
                        Status = TransactionStatusEnum.Completed,
                        Description = $"Order earning from order {order.OrderCode} (Auto-completed)"
                    };
                    await _transactionRepository.AddAsync(earningTransaction);

                    wallet.Balance += commissionableAmount;
                    wallet.TotalEarned += sellerEarning;

                    var balanceAfterEarning = wallet.Balance;

                    // Create commission deduction transaction
                    var commissionTransaction = new SellerWalletTransaction
                    {
                        WalletId = wallet.Id,
                        OrderId = order.Id,
                        TransactionType = TransactionTypeEnum.CommissionDeduction,
                        Amount = -commissionAmount,
                        BalanceBefore = balanceAfterEarning,
                        BalanceAfter = balanceAfterEarning - commissionAmount,
                        Status = TransactionStatusEnum.Completed,
                        Description = $"Commission deduction ({commissionRate}%) from order {order.OrderCode} (Auto-completed)"
                    };
                    await _transactionRepository.AddAsync(commissionTransaction);

                    wallet.Balance -= commissionAmount;
                    _walletRepository.Update(wallet);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    // Reload order with full details for notification
                    var orderId = order.Id;
                    var orderCode = order.OrderCode;
                    var reloadedOrder = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                    
                    if (reloadedOrder != null)
                    {
                        await _orderNotificationService.NotifyOrderCompletedAsync(reloadedOrder, cancellationToken);
                    }

                    _logger.LogInformation(
                        "Successfully auto-completed order {OrderCode}, Seller earned: {SellerEarning}, Commission: {CommissionAmount}",
                        orderCode,
                        sellerEarning,
                        commissionAmount);

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Failed to complete order {OrderId} in transaction", order?.Id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing order {OrderId}", order?.Id);
                return false;
            }
        }
    }
}
