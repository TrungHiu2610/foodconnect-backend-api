using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Queries
{
    public class GetCartCountQuery : IRequest<BaseResponse<CartCountResponse>>
    {
        public string? SessionId { get; set; }
    }
}
