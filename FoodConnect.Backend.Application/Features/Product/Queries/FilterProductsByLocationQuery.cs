using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class FilterProductsByLocationQuery : IRequest<BaseResponse<List<GetListProductItemResponse>>>
    {
        public double? BuyerLatitude { get; set; }
        public double? BuyerLongitude { get; set; }
    }
}
