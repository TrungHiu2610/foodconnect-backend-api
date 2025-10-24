using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Commands
{
    public class ClearCartCommand : IRequest<BaseResponse<CartResponse>>
    {
        public string? SessionId { get; set; }
    }
}
