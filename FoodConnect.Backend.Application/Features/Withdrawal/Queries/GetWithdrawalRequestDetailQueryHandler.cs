using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Withdrawal;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Queries;

public class GetWithdrawalRequestDetailQueryHandler : IRequestHandler<GetWithdrawalRequestDetailQuery, BaseResponse<WithdrawalRequestResponse>>
{
    private readonly IWithdrawalRequestRepository _withdrawalRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetWithdrawalRequestDetailQueryHandler(
        IWithdrawalRequestRepository withdrawalRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _withdrawalRepository = withdrawalRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<WithdrawalRequestResponse>> Handle(GetWithdrawalRequestDetailQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<WithdrawalRequestResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var withdrawal = await _withdrawalRepository.GetDetailByIdAsync(request.RequestId);
        if (withdrawal == null)
            return result.BuildNotFound("Withdrawal request not found");

        var response = _mapper.Map<WithdrawalRequestResponse>(withdrawal);

        return result.BuildSuccess(response, "Withdrawal request detail retrieved successfully");
    }
}
