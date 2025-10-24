using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Commands
{
    public class AddToCartCommand : IRequest<BaseResponse<CartResponse>>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public string? SessionId { get; set; }
    }
}
