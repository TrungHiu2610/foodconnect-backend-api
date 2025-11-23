using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Services;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Application.Features.Complaint.Mappers;
using FoodConnect.Backend.Application.Features.Complaint.Services;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ComplaintNotificationService _notificationService;
    private readonly WalletService _walletService;

    public ApproveComplaintCommandHandler(
        IOrderComplaintRepository complaintRepository,
        IOrderRepository orderRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ComplaintNotificationService notificationService,
        WalletService walletService)
    {
        _complaintRepository = complaintRepository;
        _orderRepository = orderRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _walletService = walletService;
    }

    public async Task<BaseResponse<ComplaintDetailDto>> Handle(ApproveComplaintCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<ComplaintDetailDto>();

        // Check authentication
        if (!_currentUserService.UserId.HasValue)
        {
            return result.BuildUnauthorized("User must be logged in");
        }

        var adminId = _currentUserService.UserId.Value;

        // Check admin role
        if (_currentUserService.Role != "Admin")
        {
            return result.BuildForbidden("Only admins can approve complaints");
        }

        // Get complaint with full details
        var complaint = await _complaintRepository.GetComplaintWithDetailsAsync(request.ComplaintId);
        if (complaint == null)
        {
            return result.BuildNotFound("Complaint not found");
        }

        // Validate complaint status
        if (complaint.Status != OrderComplaintStatusEnum.PendingAdmin && 
            complaint.Status != OrderComplaintStatusEnum.SellerResponded)
        {
            return result.BuildFail("Only complaints pending admin decision can be approved");
        }

        // Validate refund amount doesn't exceed order total
        if (request.RefundAmount > (decimal)complaint.Order.Total)
        {
            return result.BuildFail($"Refund amount cannot exceed order total ({complaint.Order.Total:N0} VND)");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Ensure buyer has a wallet
            var buyerWallet = await _walletService.GetOrCreateBuyerWalletAsync(complaint.BuyerId, cancellationToken);

            // Get seller wallet
            var sellerWallet = await _walletRepository.GetByUserIdAndTypeAsync(complaint.SellerId, WalletTypeEnum.Seller);
            if (sellerWallet == null)
            {
                return result.BuildFail("Seller wallet not found");
            }

            // Check seller has sufficient balance (considering pending balance)
            var availableBalance = sellerWallet.Balance - sellerWallet.PendingBalance;
            if (availableBalance < request.RefundAmount)
            {
                return result.BuildFail($"Seller does not have sufficient balance. Available: {availableBalance:N0} VND, Required: {request.RefundAmount:N0} VND");
            }

            // Process refund only if amount > 0
            if (request.RefundAmount > 0)
            {
                // Create buyer transaction - ComplaintRefund (+)
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
                    Description = $"Refund from complaint #{complaint.Id}",
                    Metadata = $"ComplaintId:{complaint.Id}|OrderId:{complaint.OrderId}"
                };

                // Create seller transaction - ComplaintDeduction (-)
                var sellerBalanceBefore = sellerWallet.Balance;
                sellerWallet.Balance -= request.RefundAmount;

                var sellerTransaction = new WalletTransaction
                {
                    WalletId = sellerWallet.Id,
                    OrderId = complaint.OrderId,
                    TransactionType = TransactionTypeEnum.ComplaintDeduction,
                    Amount = request.RefundAmount,
                    BalanceBefore = sellerBalanceBefore,
                    BalanceAfter = sellerWallet.Balance,
                    Status = TransactionStatusEnum.Completed,
                    Description = $"Deduction for complaint #{complaint.Id}",
                    Metadata = $"ComplaintId:{complaint.Id}|OrderId:{complaint.OrderId}"
                };

                // Update wallets
                _walletRepository.Update(buyerWallet);
                _walletRepository.Update(sellerWallet);

                // Add transactions
                await _walletTransactionRepository.AddAsync(buyerTransaction);
                await _walletTransactionRepository.AddAsync(sellerTransaction);
            }

            // Update complaint
            complaint.Status = OrderComplaintStatusEnum.Approved;
            complaint.IsApproved = true;
            complaint.ApprovedRefundAmount = request.RefundAmount;
            complaint.AdminDecisionReason = request.AdminReason;
            complaint.AdminDecidedAt = DateTime.UtcNow;
            complaint.AdminId = adminId;
            complaint.CompletedAt = DateTime.UtcNow;

            _complaintRepository.Update(complaint);

            // Update order status to Returned if refund was given
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

            // Reload complaint with updated details
            complaint = await _complaintRepository.GetComplaintWithDetailsAsync(complaint.Id);
            var complaintDto = ComplaintMapper.MapToDetailDto(complaint!);

            // Send notifications to both buyer and seller
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
