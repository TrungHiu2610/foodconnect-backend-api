using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class ReorderCommand : IRequest<BaseResponse<CartResponse>>
    {
        public Guid OrderId { get; set; }
    }
}
