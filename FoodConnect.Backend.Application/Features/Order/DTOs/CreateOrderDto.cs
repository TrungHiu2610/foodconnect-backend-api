using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Order.DTOs
{
    public class CreateOrderDto
    {
        public PaymentMethodEnum PaymentMethod { get; set; }
        public string ShippingAddressJson { get; set; } = string.Empty;
        public Dictionary<string, string>? OrderNotes { get; set; }
        
        // List of CartItem IDs to create order from
        public List<Guid> CartItemIds { get; set; } = new List<Guid>();
        
        // Optional: Promotion ID to apply discount
        public Guid? PromotionId { get; set; }
    }
}
