using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Queries
{
    public class GetCheckoutPreviewQuery : IRequest<BaseResponse<CheckoutPreviewResponse>>
    {
        public string? SessionId { get; set; }
        public List<Guid>? CartItemIds { get; set; } // Optional: checkout specific items
    }
}
