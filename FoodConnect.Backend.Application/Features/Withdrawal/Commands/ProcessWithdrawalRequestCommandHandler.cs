using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ProcessWithdrawalRequestCommandHandler : IRequestHandler<ProcessWithdrawalRequestCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly IWithdrawalRequestRepository _withdrawalRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;

    public ProcessWithdrawalRequestCommandHandler(
        IWithdrawalRequestRepository withdrawalRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        INotificationService notificationService)
    {
        _withdrawalRepository = withdrawalRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
    }

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(ProcessWithdrawalRequestCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var withdrawal = await _withdrawalRepository.GetDetailByIdAsync(request.WithdrawalRequestId);
        if (withdrawal == null)
            return result.BuildNotFound(WithdrawalValidationMessages.WITHDRAWAL_NOT_FOUND);

        if (withdrawal.Status != WithdrawalStatusEnum.Pending)
            return result.BuildFail(WithdrawalValidationMessages.OnlyStatusCanBeProcessed(withdrawal.Status.ToString(), "Pending"));

        // Validate: Approval requires proof image
        if (request.IsApproved && request.ProofImage == null)
            return result.BuildFail(WithdrawalValidationMessages.ProofImageRequired());

        // Validate: Rejection requires reason
        if (!request.IsApproved && string.IsNullOrWhiteSpace(request.RejectionReason))
            return result.BuildFail(WithdrawalValidationMessages.RejectionReasonRequired());

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (request.IsApproved)
            {
                // Upload proof image
                var proofImageUrl = await _fileStorageService.UploadFileAsync(request.ProofImage!, "Images/Withdrawals/Proofs");

                withdrawal.Status = WithdrawalStatusEnum.Completed;
                withdrawal.ProcessedBy = userId.Value;
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.CompletedAt = DateTime.UtcNow;
                withdrawal.AdminNote = request.AdminNote;
                withdrawal.ProofImageUrl = proofImageUrl;

                var pendingTransaction = await _transactionRepository.GetByWithdrawalRequestIdAsync(withdrawal.Id);
                if (pendingTransaction != null)
                {
                    pendingTransaction.Status = TransactionStatusEnum.Completed;
                    pendingTransaction.Description = TransactionDescriptions.WithdrawalCompleted(withdrawal.Id.ToString());
                    _transactionRepository.Update(pendingTransaction);
                }

                // Update wallet balances
                var wallet = await _walletRepository.GetByIdAsync(withdrawal.WalletId);
                if (wallet == null)
                    return result.BuildFail("Không tìm thấy ví");

                var balanceBefore = wallet.Balance;

                wallet.PendingBalance -= withdrawal.RequestedAmount;
                wallet.TotalWithdrawn += withdrawal.ActualAmount;

                var balanceAfter = wallet.Balance;

                var completedTransaction = await _transactionRepository.GetByWithdrawalRequestIdAsync(withdrawal.Id);
                if (completedTransaction != null)
                {
                    completedTransaction.Status = TransactionStatusEnum.Completed;
                    completedTransaction.BalanceBefore = balanceBefore;
                    completedTransaction.BalanceAfter = balanceAfter;
                    completedTransaction.Description = TransactionDescriptions.WithdrawalCompleted(withdrawal.Id.ToString());
                    _transactionRepository.Update(completedTransaction);
                }

                _walletRepository.Update(wallet);
            }

            else
            {
                withdrawal.Status = WithdrawalStatusEnum.Rejected;
                withdrawal.ProcessedBy = userId.Value;
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.RejectionReason = request.RejectionReason;
                withdrawal.AdminNote = request.AdminNote;

                var pendingTransaction = await _transactionRepository.GetByWithdrawalRequestIdAsync(withdrawal.Id);
                if (pendingTransaction != null)
                {
                    pendingTransaction.Status = TransactionStatusEnum.Failed;
                    pendingTransaction.Description = TransactionDescriptions.WithdrawalRejected(withdrawal.Id.ToString(), request.RejectionReason!);
                    _transactionRepository.Update(pendingTransaction);
                }

                var wallet = await _walletRepository.GetByIdAsync(withdrawal.WalletId);
                if (wallet != null)
                {
                    // Restore Balance and deduct from PendingBalance
                    wallet.Balance += withdrawal.RequestedAmount;
                    wallet.PendingBalance -= withdrawal.RequestedAmount;
                    _walletRepository.Update(wallet);
                }
            }

            _withdrawalRepository.Update(withdrawal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var message = request.IsApproved
                ? WithdrawalSuccessMessages.APPROVE_SUCCESS
                : WithdrawalSuccessMessages.REJECT_SUCCESS;

            // Notify seller about withdrawal processing
            try
            {
                var notificationMessage = request.IsApproved
                    ? WithdrawalNotificationMessages.SELLER_APPROVED_MESSAGE
                    : WithdrawalNotificationMessages.SellerRejectedMessage(request.RejectionReason!);

                var wallet = await _walletRepository.GetByIdAsync(withdrawal.WalletId);
                if (wallet != null)
                {
                    await _notificationService.NotifySellerWithdrawalProcessedAsync(
                        wallet.UserId,
                        withdrawal.Id,
                        request.IsApproved,
                        notificationMessage
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send notification: {ex.Message}");
            }

            return result.BuildSuccess(new CreateOrUpdateResponse
            {
                Id = withdrawal.Id,
                IsSuccess = true
            }, message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return result.BuildFail($"Xử lý yêu cầu rút tiền thất bại: {ex.Message}");
        }
    }
}
