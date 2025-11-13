using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Queries
{
    public class ValidateCartQuery : IRequest<BaseResponse<CartValidationResponse>>
    {
        public string? SessionId { get; set; }
        public List<Guid>? CartItemIds { get; set; }
        public double? BuyerLatitude { get; set; }
        public double? BuyerLongitude { get; set; }
    }
}
