using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ReportWithdrawalIssueCommandHandler : IRequestHandler<ReportWithdrawalIssueCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly IWithdrawalRequestRepository _withdrawalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public ReportWithdrawalIssueCommandHandler(
        IWithdrawalRequestRepository withdrawalRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _withdrawalRepository = withdrawalRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(ReportWithdrawalIssueCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var withdrawal = await _withdrawalRepository.GetDetailByIdAsync(request.WithdrawalRequestId);
        if (withdrawal == null)
            return result.BuildNotFound(WithdrawalValidationMessages.WITHDRAWAL_NOT_FOUND);

        // Verify ownership
        if (withdrawal.SellerId != userId.Value)
            return result.BuildForbidden();

        // Can only report issue for completed withdrawals
        if (withdrawal.Status != WithdrawalStatusEnum.Completed)
            return result.BuildFail("Chỉ có thể báo cáo vấn đề cho yêu cầu rút tiền đã hoàn thành");

        // Validate issue image is provided
        if (request.IssueImage == null)
            return result.BuildFail("Ảnh chứng minh vấn đề là bắt buộc");

        // Upload issue image
        var issueImageUrl = await _fileStorageService.UploadFileAsync(request.IssueImage, "Images/Withdrawals/Issues");

        // Update seller note with issue description
        withdrawal.SellerNote = $"[BÁO CÁO VẤN ĐỀ] {request.IssueDescription}";
        withdrawal.IssueImageUrl = issueImageUrl;
        withdrawal.Status = WithdrawalStatusEnum.UnderReview;
        withdrawal.UpdatedAtUtc = DateTime.UtcNow;
        withdrawal.UpdatedBy = userId;

        _withdrawalRepository.Update(withdrawal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result.BuildSuccess(new CreateOrUpdateResponse
        {
            Id = withdrawal.Id,
            IsSuccess = true
        }, WithdrawalSuccessMessages.REPORT_ISSUE_SUCCESS);
    }
}
