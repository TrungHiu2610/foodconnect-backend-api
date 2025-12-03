using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using FoodConnect.Backend.Application.Features.Complaint.Mappers;
using FoodConnect.Backend.Application.Features.Complaint.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Complaint.Commands.RejectComplaint;

public class RejectComplaintCommandHandler : IRequestHandler<RejectComplaintCommand, BaseResponse<ComplaintDetailDto>>
{
    private readonly IOrderComplaintRepository _complaintRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ComplaintNotificationService _notificationService;

    public RejectComplaintCommandHandler(
        IOrderComplaintRepository complaintRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        ISystemConfigRepository systemConfigRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ComplaintNotificationService notificationService)
    {
        _complaintRepository = complaintRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _systemConfigRepository = systemConfigRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task<BaseResponse<ComplaintDetailDto>> Handle(RejectComplaintCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<ComplaintDetailDto>();

        if (!_currentUserService.UserId.HasValue)
        {
            return result.BuildUnauthorized("User must be logged in");
        }

        var adminId = _currentUserService.UserId.Value;

        if (_currentUserService.Role != "Admin")
        {
            return result.BuildForbidden("Only admins can reject complaints");
        }

        var complaint = await _complaintRepository.GetComplaintWithDetailsAsync(request.ComplaintId);
        if (complaint == null)
        {
            return result.BuildNotFound("Complaint not found");
        }

        if (complaint.Status != OrderComplaintStatusEnum.PendingAdmin && 
            complaint.Status != OrderComplaintStatusEnum.SellerResponded)
        {
            return result.BuildFail("Only complaints pending admin decision can be rejected");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
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
                sellerWallet.PendingBalance -= orderTotal;

                var sellerBalanceBefore = sellerWallet.Balance;
                sellerWallet.Balance -= commissionAmount;

                var commissionTransaction = new WalletTransaction
                {
                    WalletId = sellerWallet.Id,
                    OrderId = complaint.OrderId,
                    TransactionType = TransactionTypeEnum.CommissionDeduction,
                    Amount = -commissionAmount,
                    BalanceBefore = sellerBalanceBefore,
                    BalanceAfter = sellerWallet.Balance,
                    Status = TransactionStatusEnum.Completed,
                    Description = $"Commission for COD order {complaint.Order.OrderCode} after complaint rejected",
                    Metadata = $"ComplaintId:{complaint.Id}|OrderTotal:{orderTotal}|CommissionRate:{commissionRate}%|CommissionAmount:{commissionAmount}|PaymentMethod:COD"
                };

                _walletRepository.Update(sellerWallet);
                await _walletTransactionRepository.AddAsync(commissionTransaction);
            }
            else
            {
                var sellerEarning = commissionableAmount - commissionAmount;

                var sellerBalanceBefore = sellerWallet.Balance;
                sellerWallet.Balance += sellerEarning;
                sellerWallet.TotalEarned += sellerEarning;

                var sellerTransaction = new WalletTransaction
                {
                    WalletId = sellerWallet.Id,
                    OrderId = complaint.OrderId,
                    TransactionType = TransactionTypeEnum.OrderEarning,
                    Amount = sellerEarning,
                    BalanceBefore = sellerBalanceBefore,
                    BalanceAfter = sellerWallet.Balance,
                    Status = TransactionStatusEnum.Completed,
                    Description = $"Full payout for order {complaint.Order.OrderCode} after complaint rejected",
                    Metadata = $"ComplaintId:{complaint.Id}|OrderTotal:{orderTotal}|CommissionRate:{commissionRate}%|CommissionAmount:{commissionAmount}|SellerEarning:{sellerEarning}|PaymentMethod:OnlinePayment"
                };

                _walletRepository.Update(sellerWallet);
                await _walletTransactionRepository.AddAsync(sellerTransaction);
            }

            complaint.Status = OrderComplaintStatusEnum.Rejected;
            complaint.IsApproved = false;
            complaint.ApprovedRefundAmount = 0;
            complaint.AdminDecisionReason = request.RejectionReason;
            complaint.AdminDecidedAt = DateTime.UtcNow;
            complaint.AdminId = adminId;
            complaint.CompletedAt = DateTime.UtcNow;

            _complaintRepository.Update(complaint);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            complaint = await _complaintRepository.GetComplaintWithDetailsAsync(complaint.Id);
            var complaintDto = ComplaintMapper.MapToDetailDto(complaint!);

            await _notificationService.NotifyComplaintRejectedAsync(complaint!, cancellationToken);

            return result.BuildSuccess(complaintDto, "Complaint rejected successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return result.BuildFail($"Failed to reject complaint: {ex.Message}");
        }
    }
}
