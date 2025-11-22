using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wallet.Queries;

public class GetWalletTransactionsQueryHandler : IRequestHandler<GetWalletTransactionsQuery, BaseResponse<PaginatedList<WalletTransactionResponse>>>
{
    private readonly ISellerWalletRepository _walletRepository;
    private readonly ISellerWalletTransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetWalletTransactionsQueryHandler(
        ISellerWalletRepository walletRepository,
        ISellerWalletTransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<PaginatedList<WalletTransactionResponse>>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<PaginatedList<WalletTransactionResponse>>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var wallet = await _walletRepository.GetBySellerIdAsync(userId.Value);
        if (wallet == null)
            return result.BuildNotFound("Wallet not found");

        var transactions = await _transactionRepository.GetByWalletIdAsync(wallet.Id, request.PageNumber, request.PageSize, request.Type);
        var totalCount = await _transactionRepository.GetTransactionCountByWalletIdAsync(wallet.Id);

        var transactionResponses = _mapper.Map<List<WalletTransactionResponse>>(transactions);

        var paginatedList = new PaginatedList<WalletTransactionResponse>(
            transactionResponses,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return result.BuildSuccess(paginatedList, "Transactions retrieved successfully");
    }
}
