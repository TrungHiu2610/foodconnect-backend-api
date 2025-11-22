using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Withdrawal;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Queries;

public class GetWithdrawalRequestsQueryHandler : IRequestHandler<GetWithdrawalRequestsQuery, BaseResponse<PaginatedList<WithdrawalRequestListResponse>>>
{
    private readonly IWithdrawalRequestRepository _withdrawalRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetWithdrawalRequestsQueryHandler(
        IWithdrawalRequestRepository withdrawalRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _withdrawalRepository = withdrawalRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<PaginatedList<WithdrawalRequestListResponse>>> Handle(GetWithdrawalRequestsQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<PaginatedList<WithdrawalRequestListResponse>>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        WithdrawalStatusEnum? status = request.Status.HasValue 
            ? (WithdrawalStatusEnum)request.Status.Value 
            : null;

        var withdrawals = await _withdrawalRepository.GetAllWithFiltersAsync(
            status,
            request.SellerId,
            request.FromDate,
            request.ToDate,
            request.PageNumber,
            request.PageSize
        );

        var totalCount = await _withdrawalRepository.GetCountByFiltersAsync(
            status,
            request.SellerId,
            request.FromDate,
            request.ToDate
        );

        var withdrawalResponses = _mapper.Map<List<WithdrawalRequestListResponse>>(withdrawals);

        var paginatedList = new PaginatedList<WithdrawalRequestListResponse>(
            withdrawalResponses,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return result.BuildSuccess(paginatedList, "Withdrawal requests retrieved successfully");
    }
}
