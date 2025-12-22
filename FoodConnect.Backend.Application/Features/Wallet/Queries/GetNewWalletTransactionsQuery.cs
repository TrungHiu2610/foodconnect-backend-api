using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wallet.Queries;

public class GetNewWalletTransactionsQuery : IRequest<BaseResponse<PaginatedList<WalletTransactionResponse>>>
{
    public int WalletType { get; set; } // 0 = Buyer, 1 = Seller
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? TransactionType { get; set; }
}

public class GetNewWalletTransactionsQueryHandler : IRequestHandler<GetNewWalletTransactionsQuery, BaseResponse<PaginatedList<WalletTransactionResponse>>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetNewWalletTransactionsQueryHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<PaginatedList<WalletTransactionResponse>>> Handle(GetNewWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<PaginatedList<WalletTransactionResponse>>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var walletType = (WalletTypeEnum)request.WalletType;
        var wallet = await _walletRepository.GetByUserIdAndTypeAsync(userId.Value, walletType);
        
        if (wallet == null)
            return result.BuildNotFound("Wallet not found");

        TransactionTypeEnum? transactionType = request.TransactionType.HasValue 
            ? (TransactionTypeEnum)request.TransactionType.Value 
            : null;

        var transactions = await _transactionRepository.GetByWalletIdAsync(
            wallet.Id, 
            request.PageNumber, 
            request.PageSize, 
            transactionType);

        var totalCount = await _transactionRepository.CountByWalletIdAsync(wallet.Id, transactionType);

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
