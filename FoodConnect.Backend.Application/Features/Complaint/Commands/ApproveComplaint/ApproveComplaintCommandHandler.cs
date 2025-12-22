using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Services;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Application.Features.Complaint.Mappers;
using FoodConnect.Backend.Application.Features.Complaint.Services;
using FoodConnect.Backend.Application.Features.Wallet.Queries;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Commands.ApproveComplaint;

public class ApproveComplaintCommandHandler : IRequestHandler<ApproveComplaintCommand, BaseResponse<ComplaintDetailDto>>
{
    private readonly IOrderComplaintRepository _complaintRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ComplaintNotificationService _notificationService;
    private readonly WalletService _walletService;

    public ApproveComplaintCommandHandler(
        IOrderComplaintRepository complaintRepository,
        IOrderRepository orderRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        ISystemConfigRepository systemConfigRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ComplaintNotificationService notificationService,
        WalletService walletService)
    {
        _complaintRepository = complaintRepository;
        _orderRepository = orderRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _systemConfigRepository = systemConfigRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _walletService = walletService;
    }

    public async Task<BaseResponse<ComplaintDetailDto>> Handle(ApproveComplaintCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<ComplaintDetailDto>();

        if (!_currentUserService.UserId.HasValue)
        {
            return result.BuildUnauthorized("User must be logged in");
        }

        var adminId = _currentUserService.UserId.Value;

        if (_currentUserService.Role != "Admin")
        {
            return result.BuildForbidden("Only admins can approve complaints");
        }

        var complaint = await _complaintRepository.GetComplaintWithDetailsAsync(request.ComplaintId);
        if (complaint == null)
        {
            return result.BuildNotFound("Complaint not found");
        }

        if (complaint.Status != OrderComplaintStatusEnum.PendingAdmin && 
            complaint.Status != OrderComplaintStatusEnum.SellerResponded)
        {
            return result.BuildFail("Only complaints pending admin decision can be approved");
        }

        if (request.RefundAmount > (decimal)complaint.Order.Total)
        {
            return result.BuildFail($"Refund amount cannot exceed order total ({complaint.Order.Total:N0} VND)");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var buyerWallet = await _walletService.GetOrCreateBuyerWalletAsync(complaint.BuyerId, cancellationToken);

            var sellerWallet = await _walletRepository.GetByUserIdAndTypeAsync(complaint.SellerId, WalletTypeEnum.Seller);
            if (sellerWallet == null)
            {
                return result.BuildFail("Seller wallet not found");
            }

            var commissionRate = await _systemConfigRepository.GetCommissionRateAsync();
            var orderTotal = (decimal)complaint.Order.Total;
            var shippingFee = (decimal)complaint.Order.ShippingFee;
            var commissionableAmount = orderTotal - shippingFee;
            var commissionAmount = commissionableAmount * (commissionRate / 100);

            if (complaint.Order.PaymentMethod == PaymentMethodEnum.COD)
            {
                var availableBalance = sellerWallet.Balance - sellerWallet.PendingBalance;
                if (availableBalance < request.RefundAmount)
                {
                    return result.BuildFail($"Seller không đủ số dư để hoàn tiền. Cần: {request.RefundAmount:N0} VNĐ, Có: {availableBalance:N0} VNĐ");
                }

                sellerWallet.PendingBalance -= orderTotal;

                var sellerBalanceBefore = sellerWallet.Balance;
                sellerWallet.Balance -= (request.RefundAmount + commissionAmount);

                if (request.RefundAmount > 0)
                {
                    var buyerBalanceBefore = buyerWallet.Balance;
                    buyerWallet.Balance += request.RefundAmount;

                    var buyerTransaction = new WalletTransaction
                    {
                        WalletId = buyerWallet.Id,
                        OrderId = complaint.OrderId,
                        TransactionType = TransactionTypeEnum.ComplaintRefund,
                        Amount = request.RefundAmount,
                        BalanceBefore = buyerBalanceBefore,
                        BalanceAfter = buyerWallet.Balance,
                        Status = TransactionStatusEnum.Completed,
                        Description = $"Refund for COD order {complaint.Order.OrderCode} after complaint approved",
                        Metadata = $"ComplaintId:{complaint.Id}|OrderId:{complaint.OrderId}|RefundAmount:{request.RefundAmount}|PaymentMethod:COD"
                    };
                    await _walletTransactionRepository.AddAsync(buyerTransaction);
                }

                var sellerDeduction = request.RefundAmount + commissionAmount;
                var sellerTransaction = new WalletTransaction
                {
                    WalletId = sellerWallet.Id,
                    OrderId = complaint.OrderId,
                    TransactionType = TransactionTypeEnum.ComplaintDeduction,
                    Amount = -sellerDeduction,
                    BalanceBefore = sellerBalanceBefore,
                    BalanceAfter = sellerWallet.Balance,
                    Status = TransactionStatusEnum.Completed,
                    Description = $"Refund + Commission for COD complaint #{complaint.Id}",
                    Metadata = $"ComplaintId:{complaint.Id}|OrderTotal:{orderTotal}|RefundAmount:{request.RefundAmount}|CommissionRate:{commissionRate}%|CommissionAmount:{commissionAmount}|PaymentMethod:COD"
                };
                await _walletTransactionRepository.AddAsync(sellerTransaction);

                _walletRepository.Update(buyerWallet);
                _walletRepository.Update(sellerWallet);
            }
            else
            {
                
                if (request.RefundAmount > 0)
                {
                    var buyerBalanceBefore = buyerWallet.Balance;
                    buyerWallet.Balance += request.RefundAmount;

                    var buyerTransaction = new WalletTransaction
                    {
                        WalletId = buyerWallet.Id,
                        OrderId = complaint.OrderId,
                        TransactionType = TransactionTypeEnum.ComplaintRefund,
                        Amount = request.RefundAmount,
                        BalanceBefore = buyerBalanceBefore,
                        BalanceAfter = buyerWallet.Balance,
                        Status = TransactionStatusEnum.Completed,
                        Description = $"Refund for approved complaint #{complaint.Id}",
                        Metadata = $"ComplaintId:{complaint.Id}|OrderId:{complaint.OrderId}|RefundAmount:{request.RefundAmount}|PaymentMethod:OnlinePayment"
                    };

                    _walletRepository.Update(buyerWallet);
                    await _walletTransactionRepository.AddAsync(buyerTransaction);
                }

                var sellerPayout = commissionableAmount - request.RefundAmount - commissionAmount;
                
                if (sellerPayout > 0)
                {
                    var sellerBalanceBefore = sellerWallet.Balance;
                    sellerWallet.Balance += sellerPayout;
                    sellerWallet.TotalEarned += sellerPayout;

                    var sellerTransaction = new WalletTransaction
                    {
                        WalletId = sellerWallet.Id,
                        OrderId = complaint.OrderId,
                        TransactionType = TransactionTypeEnum.OrderEarning,
                        Amount = sellerPayout,
                        BalanceBefore = sellerBalanceBefore,
                        BalanceAfter = sellerWallet.Balance,
                        Status = TransactionStatusEnum.Completed,
                        Description = $"Payout for order {complaint.Order.OrderCode} after complaint approved (partial refund to buyer)",
                        Metadata = $"ComplaintId:{complaint.Id}|OrderTotal:{orderTotal}|RefundAmount:{request.RefundAmount}|CommissionRate:{commissionRate}%|CommissionAmount:{commissionAmount}|SellerPayout:{sellerPayout}|PaymentMethod:OnlinePayment"
                    };

                    _walletRepository.Update(sellerWallet);
                    await _walletTransactionRepository.AddAsync(sellerTransaction);
                }
                else if (sellerPayout < 0)
                {
                    return result.BuildFail($"Invalid refund amount. Refund cannot exceed order total minus commission. Maximum refundable: {commissionableAmount - commissionAmount:N0} VND");
                }
            }

            complaint.Status = OrderComplaintStatusEnum.Approved;
            complaint.IsApproved = true;
            complaint.ApprovedRefundAmount = request.RefundAmount;
            complaint.AdminDecisionReason = request.AdminReason;
            complaint.AdminDecidedAt = DateTime.UtcNow;
            complaint.AdminId = adminId;
            complaint.CompletedAt = DateTime.UtcNow;

            _complaintRepository.Update(complaint);

            if (request.RefundAmount > 0)
            {
                var order = await _orderRepository.GetByIdAsync(complaint.OrderId);
                if (order != null)
                {
                    order.Status = OrderStatusEnum.Returned;
                    order.CompletedAt = DateTime.UtcNow;
                    _orderRepository.Update(order);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            complaint = await _complaintRepository.GetComplaintWithDetailsAsync(complaint.Id);
            var complaintDto = ComplaintMapper.MapToDetailDto(complaint!);

            await _notificationService.NotifyComplaintApprovedAsync(complaint!, cancellationToken);

            return result.BuildSuccess(complaintDto, "Complaint approved successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return result.BuildFail($"Failed to approve complaint: {ex.Message}");
        }
    }
}
