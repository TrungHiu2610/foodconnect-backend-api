using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class CreateOrderCommand : IRequest<BaseResponse<List<OrderDetailDto>>>
    {
        public PaymentMethodEnum PaymentMethod { get; set; }
        public string ShippingAddressJson { get; set; } = string.Empty;
        public Dictionary<string, string>? OrderNotes { get; set; }
        public List<Guid> CartItemIds { get; set; } = new List<Guid>();
        public Guid? PromotionId { get; set; }
    }
}
