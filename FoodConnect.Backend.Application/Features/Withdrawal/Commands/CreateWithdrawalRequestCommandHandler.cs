using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class CreateWithdrawalRequestCommandHandler : IRequestHandler<CreateWithdrawalRequestCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly ISellerWalletRepository _walletRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRepository;
    private readonly ISellerWalletTransactionRepository _transactionRepository;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;

    public CreateWithdrawalRequestCommandHandler(
        ISellerWalletRepository walletRepository,
        IWithdrawalRequestRepository withdrawalRepository,
        ISellerWalletTransactionRepository transactionRepository,
        ISystemConfigRepository systemConfigRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationService notificationService)
    {
        _walletRepository = walletRepository;
        _withdrawalRepository = withdrawalRepository;
        _transactionRepository = transactionRepository;
        _systemConfigRepository = systemConfigRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(CreateWithdrawalRequestCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var wallet = await _walletRepository.GetBySellerIdAsync(userId.Value);
        if (wallet == null)
            return result.BuildNotFound("Không tìm thấy ví");

        if (wallet.Status != SellerWalletStatusEnum.Active)
            return result.BuildFail("Ví chưa kích hoạt");

        var minWithdrawAmount = await _systemConfigRepository.GetMinWithdrawAmountAsync();
        if (request.RequestedAmount < minWithdrawAmount)
            return result.BuildFail($"Số tiền rút tối thiểu là {minWithdrawAmount:N0} VND");

        var availableBalance = wallet.Balance - wallet.PendingBalance;
        if (availableBalance < request.RequestedAmount)
            return result.BuildFail($"Số dư không đủ. Số dư khả dùng: {availableBalance:N0} VND");

        var hasPending = await _withdrawalRepository.HasPendingWithdrawalAsync(wallet.Id);
        if (hasPending)
            return result.BuildFail("Bạn đã có một yêu cầu rút tiền đang chờ xử lý");

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var processingFee = 0m;
            var actualAmount = request.RequestedAmount - processingFee;

            var withdrawalRequest = new WithdrawalRequest
            {
                WalletId = wallet.Id,
                SellerId = userId.Value,
                RequestedAmount = request.RequestedAmount,
                ActualAmount = actualAmount,
                ProcessingFee = processingFee,
                Status = WithdrawalStatusEnum.Pending,
                PaymentMethod = (PaymentMethodEnum)request.PaymentMethod,
                PaymentAccountNumber = request.PaymentAccountNumber,
                PaymentAccountName = request.PaymentAccountName,
                RequestedAt = DateTime.UtcNow,
                SellerNote = request.SellerNote
            };

            await _withdrawalRepository.AddAsync(withdrawalRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var balanceBefore = wallet.Balance;
            var walletTransaction = new SellerWalletTransaction
            {
                WalletId = wallet.Id,
                WithdrawalRequestId = withdrawalRequest.Id,
                TransactionType = TransactionTypeEnum.Withdraw,
                Amount = -request.RequestedAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceBefore,
                Status = TransactionStatusEnum.Pending,
                Description = TransactionDescriptions.WithdrawalPending(withdrawalRequest.Id.ToString())
            };

            await _transactionRepository.AddAsync(walletTransaction);

            wallet.PendingBalance += request.RequestedAmount;
            _walletRepository.Update(wallet);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Notify admins about new withdrawal request
            try
            {
                var sellerName = _currentUserService.UserName ?? "Seller";
                await _notificationService.NotifyAdminNewWithdrawalRequestAsync(
                    withdrawalRequest.Id,
                    sellerName,
                    request.RequestedAmount
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send notification: {ex.Message}");
            }

            return result.BuildSuccess(new CreateOrUpdateResponse
            {
                Id = withdrawalRequest.Id,
                IsSuccess = true
            }, WithdrawalSuccessMessages.CREATE_SUCCESS);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return result.BuildFail($"Tạo yêu cầu rút tiền thất bại: {ex.Message}");
        }
    }
}
