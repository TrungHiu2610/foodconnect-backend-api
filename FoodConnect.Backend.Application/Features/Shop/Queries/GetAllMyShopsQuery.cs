using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetAllMyShopsQuery : IRequest<BaseResponse<List<ShopResponse>>>
    {
    }
}
