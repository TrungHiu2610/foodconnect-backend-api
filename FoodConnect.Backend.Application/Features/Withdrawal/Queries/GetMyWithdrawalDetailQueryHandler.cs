using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Withdrawal;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Queries;

public class GetMyWithdrawalDetailQueryHandler : IRequestHandler<GetMyWithdrawalDetailQuery, BaseResponse<WithdrawalRequestResponse>>
{
    private readonly IWithdrawalRequestRepository _withdrawalRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetMyWithdrawalDetailQueryHandler(
        IWithdrawalRequestRepository withdrawalRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _withdrawalRepository = withdrawalRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<WithdrawalRequestResponse>> Handle(GetMyWithdrawalDetailQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<WithdrawalRequestResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var withdrawal = await _withdrawalRepository.GetDetailByIdAsync(request.RequestId);
        if (withdrawal == null)
            return result.BuildNotFound("Withdrawal request not found");

        // Verify ownership through wallet
        if (withdrawal.Wallet == null || withdrawal.Wallet.UserId != userId.Value)
            return result.BuildForbidden();

        var response = _mapper.Map<WithdrawalRequestResponse>(withdrawal);

        return result.BuildSuccess(response, "Withdrawal request detail retrieved successfully");
    }
}
