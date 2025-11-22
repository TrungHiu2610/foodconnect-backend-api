using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetShopDetailForBuyerQuery : IRequest<BaseResponse<ShopDetailForBuyerResponse>>
    {
        public Guid ShopId { get; set; }
        public double? UserLatitude { get; set; }
        public double? UserLongitude { get; set; }
    }
}
