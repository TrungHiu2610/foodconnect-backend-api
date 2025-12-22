using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetFeaturedShopsQuery : IRequest<BaseResponse<List<ShopListForBuyerResponse>>>
    {
        public int Limit { get; set; } = 10;
        public double? UserLatitude { get; set; }
        public double? UserLongitude { get; set; }
    }
}
