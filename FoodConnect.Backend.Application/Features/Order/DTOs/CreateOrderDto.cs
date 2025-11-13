using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Order.DTOs
{
    public class CreateOrderDto
    {
        public PaymentMethodEnum PaymentMethod { get; set; }
        public string ShippingAddressJson { get; set; } = string.Empty;
        public string? Notes { get; set; }
        // List of CartItem IDs to create order from
        public List<Guid> CartItemIds { get; set; } = new List<Guid>();
    }
}
