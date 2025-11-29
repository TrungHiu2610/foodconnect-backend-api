using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ResolveWithdrawalIssueCommandHandler : IRequestHandler<ResolveWithdrawalIssueCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly IWithdrawalRequestRepository _withdrawalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;

    public ResolveWithdrawalIssueCommandHandler(
        IWithdrawalRequestRepository withdrawalRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        INotificationService notificationService)
    {
        _withdrawalRepository = withdrawalRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
    }

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(ResolveWithdrawalIssueCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var withdrawal = await _withdrawalRepository.GetDetailByIdAsync(request.WithdrawalRequestId);
        if (withdrawal == null)
            return result.BuildNotFound(WithdrawalValidationMessages.WITHDRAWAL_NOT_FOUND);

        // Must be in UnderReview status
        if (withdrawal.Status != WithdrawalStatusEnum.UnderReview)
            return result.BuildFail("Chỉ có thể giải quyết vấn đề cho yêu cầu đang được xem xét");

        // Validate: NewProofImage is required
        if (request.NewProofImage == null)
            return result.BuildFail("Ảnh chứng từ là bắt buộc khi giải quyết vấn đề");

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Upload new proof image
            var proofImageUrl = await _fileStorageService.UploadFileAsync(request.NewProofImage, "Images/Withdrawals/Proofs");

            // Update based on resolution
            if (request.Resolution.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
            {
                withdrawal.Status = WithdrawalStatusEnum.Completed;
                withdrawal.AdminNote = $"[ĐÃ GIẢI QUYẾT] {request.AdminNote}";
                withdrawal.ProofImageUrl = proofImageUrl;
            }
            else if (request.Resolution.Equals("NeedsReinvestigation", StringComparison.OrdinalIgnoreCase))
            {
                return result.BuildFail("NeedsReinvestigation không được hỗ trợ. Vui lòng sử dụng trạng thái 'Resolved'.");
            }
            else
            {
                return result.BuildFail("Loại giải quyết không hợp lệ. Chỉ sử dụng 'Resolved'.");
            }

            withdrawal.ProcessedAt = DateTime.UtcNow;
            withdrawal.ProcessedBy = userId;
            withdrawal.UpdatedAtUtc = DateTime.UtcNow;
            withdrawal.UpdatedBy = userId;

            _withdrawalRepository.Update(withdrawal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Notify seller about issue resolution
            try
            {
                if (withdrawal.Wallet != null)
                {
                    await _notificationService.NotifySellerWithdrawalResolvedAsync(
                        withdrawal.Wallet.UserId,
                        withdrawal.Id,
                        WithdrawalNotificationMessages.SELLER_ISSUE_RESOLVED_MESSAGE
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
            }, WithdrawalSuccessMessages.RESOLVE_ISSUE_SUCCESS);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return result.BuildFail($"Giải quyết vấn đề thất bại: {ex.Message}");
        }
    }
}
