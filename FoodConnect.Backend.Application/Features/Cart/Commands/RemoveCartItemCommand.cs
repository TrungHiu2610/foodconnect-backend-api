using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Commands
{
    public class RemoveCartItemCommand : IRequest<BaseResponse<CartResponse>>
    {
        public List<Guid> CartItemIds { get; set; } = new List<Guid>();
        public string? SessionId { get; set; }
    }
}
