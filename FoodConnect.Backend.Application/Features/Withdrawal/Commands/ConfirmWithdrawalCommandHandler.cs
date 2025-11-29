using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ConfirmWithdrawalCommandHandler : IRequestHandler<ConfirmWithdrawalCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly IWithdrawalRequestRepository _withdrawalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ConfirmWithdrawalCommandHandler(
        IWithdrawalRequestRepository withdrawalRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _withdrawalRepository = withdrawalRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(ConfirmWithdrawalCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var withdrawal = await _withdrawalRepository.GetDetailByIdAsync(request.WithdrawalRequestId);
        if (withdrawal == null)
            return result.BuildNotFound("Withdrawal request not found");

        // Verify ownership through wallet
        if (withdrawal.Wallet == null || withdrawal.Wallet.UserId != userId.Value)
            return result.BuildFail("You are not authorized to confirm this withdrawal request");

        if (withdrawal.Status != WithdrawalStatusEnum.Completed)
            return result.BuildFail($"Only completed withdrawal requests can be confirmed. Current status: {withdrawal.Status}");

        // Mark as confirmed (no status change needed, just record confirmation)
        // You can add a ConfirmedAt field to WithdrawalRequest entity if needed
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result.BuildSuccess(new CreateOrUpdateResponse
        {
            Id = withdrawal.Id,
            IsSuccess = true
        }, "Withdrawal confirmed successfully");
    }
}
