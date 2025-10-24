using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Commands
{
    public class UpdateCartItemCommand : IRequest<BaseResponse<CartResponse>>
    {
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }
        public string? SessionId { get; set; }
    }
}
