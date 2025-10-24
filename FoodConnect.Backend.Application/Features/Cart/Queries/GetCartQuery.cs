using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Queries
{
    public class GetCartQuery : IRequest<BaseResponse<CartResponse>>
    {
        public string? SessionId { get; set; }
    }
}
