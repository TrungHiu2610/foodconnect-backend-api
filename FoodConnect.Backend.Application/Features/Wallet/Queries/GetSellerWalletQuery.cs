using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wallet.Queries;

public class GetSellerWalletQuery : IRequest<BaseResponse<SellerWalletResponse>>
{
}
