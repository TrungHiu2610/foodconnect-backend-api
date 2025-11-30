using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class ConfirmOrderReceivedCommandHandler : IRequestHandler<ConfirmOrderReceivedCommand, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletTransactionRepository _transactionRepository;
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;

        public ConfirmOrderReceivedCommandHandler(
            IOrderRepository orderRepository,
            IWalletRepository walletRepository,
            IWalletTransactionRepository transactionRepository,
            ISystemConfigRepository systemConfigRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            OrderNotificationService orderNotificationService)
        {
            _orderRepository = orderRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _systemConfigRepository = systemConfigRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _orderNotificationService = orderNotificationService;
        }

        public async Task<BaseResponse<OrderDetailDto>> Handle(ConfirmOrderReceivedCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OrderDetailDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var buyerId = _currentUserService.UserId.Value;

            // Get order
            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            // Check if order belongs to buyer
            if (order.BuyerId != buyerId)
            {
                return result.BuildForbidden("You don't have permission to confirm this order");
            }

            // Check if order is delivered
            if (order.Status != OrderStatusEnum.Delivered)
            {
                return result.BuildFail("Only delivered orders can be confirmed as received");
            }

            if (order.PaymentMethod != PaymentMethodEnum.COD && order.PaymentStatus != PaymentStatusEnum.Paid)
            {
                return result.BuildFail("Order must be paid before completing");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                order.Status = OrderStatusEnum.Completed;
                order.CompletedAt = DateTime.UtcNow;
                _orderRepository.Update(order);

                var sellerId = order.Shop.UserId;
                var wallet = await _walletRepository.GetByUserIdAndTypeAsync(sellerId, WalletTypeEnum.Seller);

                if (wallet == null)
                {
                    wallet = new Domain.Entities.Wallet
                    {
                        UserId = sellerId,
                        WalletType = WalletTypeEnum.Seller,
                        Balance = 0,
                        TotalEarned = 0,
                        TotalWithdrawn = 0,
                        PendingBalance = 0,
                        Status = WalletStatusEnum.Active
                    };
                    await _walletRepository.AddAsync(wallet);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                var commissionRate = await _systemConfigRepository.GetCommissionRateAsync();
                var commissionableAmount = (decimal)(order.Total - order.ShippingFee);
                var commissionAmount = commissionableAmount * (commissionRate / 100);
                var sellerEarning = commissionableAmount - commissionAmount;

                // Different logic for COD vs Online Payment
                if (order.PaymentMethod == PaymentMethodEnum.COD)
                {
                    // COD: Seller already received cash, now must pay commission and release pending
                    var orderTotal = (decimal)order.Total;
                    
                    // Deduct commission from wallet balance
                    var balanceBefore = wallet.Balance;
                    wallet.Balance -= commissionAmount;
                    
                    // Release pending balance
                    wallet.PendingBalance -= orderTotal;

                    var commissionTransaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        OrderId = order.Id,
                        TransactionType = TransactionTypeEnum.CommissionDeduction,
                        Amount = -commissionAmount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = wallet.Balance,
                        Status = TransactionStatusEnum.Completed,
                        Description = $"Commission deduction ({commissionRate}%) from COD order {order.OrderCode}",
                        Metadata = $"OrderTotal:{orderTotal}|CommissionRate:{commissionRate}%|CommissionAmount:{commissionAmount}|PaymentMethod:COD"
                    };
                    await _transactionRepository.AddAsync(commissionTransaction);
                }
                else
                {
                    // Online Payment: Money from Admin, add to seller wallet
                    var balanceBefore = wallet.Balance;

                    var earningTransaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        OrderId = order.Id,
                        TransactionType = TransactionTypeEnum.OrderEarning,
                        Amount = commissionableAmount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceBefore + commissionableAmount,
                        Status = TransactionStatusEnum.Completed,
                        Description = $"Order earning from order {order.OrderCode}",
                        Metadata = $"OrderTotal:{order.Total}|CommissionableAmount:{commissionableAmount}|PaymentMethod:OnlinePayment"
                    };
                    await _transactionRepository.AddAsync(earningTransaction);

                    wallet.Balance += commissionableAmount;
                    wallet.TotalEarned += sellerEarning;

                    var balanceAfterEarning = wallet.Balance;

                    var commissionTransaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        OrderId = order.Id,
                        TransactionType = TransactionTypeEnum.CommissionDeduction,
                        Amount = -commissionAmount,
                        BalanceBefore = balanceAfterEarning,
                        BalanceAfter = balanceAfterEarning - commissionAmount,
                        Status = TransactionStatusEnum.Completed,
                        Description = $"Commission deduction ({commissionRate}%) from order {order.OrderCode}",
                        Metadata = $"CommissionRate:{commissionRate}%|CommissionAmount:{commissionAmount}|PaymentMethod:OnlinePayment"
                    };
                    await _transactionRepository.AddAsync(commissionTransaction);

                    wallet.Balance -= commissionAmount;
                }

                _walletRepository.Update(wallet);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
                await _orderNotificationService.NotifyOrderCompletedAsync(order!, cancellationToken);

                var orderDto = OrderMapper.MapToDetailDto(order!);
                return result.BuildSuccess(orderDto, "Order confirmed as received successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result.BuildFail($"Failed to confirm order: {ex.Message}");
            }
        }
    }
}
