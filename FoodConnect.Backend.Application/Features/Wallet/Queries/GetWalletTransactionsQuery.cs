using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wallet.Queries;

public class GetWalletTransactionsQuery : IRequest<BaseResponse<PaginatedList<WalletTransactionResponse>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public TransactionTypeEnum? Type { get; set; }
}
