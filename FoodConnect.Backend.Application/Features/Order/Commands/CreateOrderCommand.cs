using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    /// <summary>
    /// Command to create orders from cart items.
    /// Orders will be automatically split by (Shop + DeliveryType).
    /// DeliveryType is determined from Product.Category.DeliveryType.
    /// </summary>
    public class CreateOrderCommand : IRequest<BaseResponse<List<OrderDetailDto>>>
    {
        public PaymentMethodEnum PaymentMethod { get; set; }
        // DeliveryType removed - will be auto-determined from Product.Category
        public string ShippingAddressJson { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<Guid> CartItemIds { get; set; } = new List<Guid>();
    }
}
