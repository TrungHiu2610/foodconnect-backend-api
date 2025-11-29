using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class CancelWithdrawalRequestCommandHandler : IRequestHandler<CancelWithdrawalRequestCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly IWithdrawalRequestRepository _withdrawalRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CancelWithdrawalRequestCommandHandler(
        IWithdrawalRequestRepository withdrawalRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _withdrawalRepository = withdrawalRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(CancelWithdrawalRequestCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var withdrawal = await _withdrawalRepository.GetDetailByIdAsync(request.WithdrawalRequestId);
        if (withdrawal == null)
            return result.BuildNotFound(WithdrawalValidationMessages.WITHDRAWAL_NOT_FOUND);

        var wallet = await _walletRepository.GetByIdAsync(withdrawal.WalletId);
        if (wallet == null || wallet.UserId != userId.Value)
            return result.BuildForbidden(WithdrawalValidationMessages.UNAUTHORIZED);

        if (withdrawal.Status != WithdrawalStatusEnum.Pending)
            return result.BuildFail(WithdrawalValidationMessages.OnlyStatusCanBeProcessed(withdrawal.Status.ToString(), "Pending"));

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            withdrawal.Status = WithdrawalStatusEnum.Cancelled;
            _withdrawalRepository.Update(withdrawal);

            var pendingTransaction = await _transactionRepository.GetByWithdrawalRequestIdAsync(withdrawal.Id);
            if (pendingTransaction != null)
            {
                pendingTransaction.Status = TransactionStatusEnum.Cancelled;
                pendingTransaction.Description = TransactionDescriptions.WithdrawalCancelled(withdrawal.Id.ToString());
                _transactionRepository.Update(pendingTransaction);
            }

            // Restore Balance and deduct from PendingBalance
            wallet.Balance += withdrawal.RequestedAmount;
            wallet.PendingBalance -= withdrawal.RequestedAmount;
            _walletRepository.Update(wallet);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return result.BuildSuccess(new CreateOrUpdateResponse
            {
                Id = withdrawal.Id,
                IsSuccess = true
            }, WithdrawalSuccessMessages.CANCEL_SUCCESS);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return result.BuildFail($"Hủy yêu cầu rút tiền thất bại: {ex.Message}");
        }
    }
}
