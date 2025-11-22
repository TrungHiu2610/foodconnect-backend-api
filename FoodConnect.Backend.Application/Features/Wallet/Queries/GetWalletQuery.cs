using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wallet.Queries;

public class GetWalletQuery : IRequest<BaseResponse<WalletResponse>>
{
    public int WalletType { get; set; } // 0 = Buyer, 1 = Seller
}
