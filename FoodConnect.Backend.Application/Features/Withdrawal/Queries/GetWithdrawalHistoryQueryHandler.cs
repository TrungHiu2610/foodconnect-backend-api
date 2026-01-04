using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Withdrawal;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Queries;

public class GetWithdrawalHistoryQueryHandler : IRequestHandler<GetWithdrawalHistoryQuery, BaseResponse<PaginatedList<WithdrawalRequestListResponse>>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetWithdrawalHistoryQueryHandler(
        IWalletRepository walletRepository,
        IWithdrawalRequestRepository withdrawalRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _walletRepository = walletRepository;
        _withdrawalRepository = withdrawalRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<PaginatedList<WithdrawalRequestListResponse>>> Handle(GetWithdrawalHistoryQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<PaginatedList<WithdrawalRequestListResponse>>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var wallet = await _walletRepository.GetByUserIdAndTypeAsync(userId.Value, WalletTypeEnum.Seller);
        
        if (wallet == null)
        {
            wallet = await _walletRepository.GetByUserIdAndTypeAsync(userId.Value, WalletTypeEnum.Buyer);
        }

        if (wallet == null)
            return result.BuildNotFound("Wallet not found");

        WithdrawalStatusEnum? status = request.Status.HasValue 
            ? (WithdrawalStatusEnum)request.Status.Value 
            : null;

        var withdrawals = await _withdrawalRepository.GetAllWithFiltersAsync(
            status,
            userId.Value,
            null,
            null,
            request.PageNumber,
            request.PageSize
        );

        var totalCount = await _withdrawalRepository.GetCountByFiltersAsync(
            status,
            userId.Value,
            null,
            null
        );

        var withdrawalResponses = _mapper.Map<List<WithdrawalRequestListResponse>>(withdrawals);

        var paginatedList = new PaginatedList<WithdrawalRequestListResponse>(
            withdrawalResponses,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return result.BuildSuccess(paginatedList, "Withdrawal history retrieved successfully");
    }
}
