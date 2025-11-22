using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Order.DTOs
{
    public class OrderDetailDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public double SubTotal { get; set; }
        public double ShippingFee { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public OrderStatusEnum Status { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public string ShippingAddressJson { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CancelReason { get; set; }
        public string? RejectionReason { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? PreparedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        // Buyer info
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        
        // Shop info
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? ShopPhone { get; set; }
        public string? ShopAddress { get; set; }
        
        // Order items
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public OrderReviewStatusEnum? ReviewStatus { get; set; }
    }
}
