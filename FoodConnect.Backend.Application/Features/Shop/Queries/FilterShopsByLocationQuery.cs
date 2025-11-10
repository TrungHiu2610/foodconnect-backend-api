using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class FilterShopsByLocationQuery : IRequest<BaseResponse<List<ShopResponse>>>
    {
        public DeliveryTypeEnum? DeliveryType { get; set; }
        public double? BuyerLatitude { get; set; }
        public double? BuyerLongitude { get; set; }
    }
}
