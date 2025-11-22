using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Order.DTOs
{
    public class OrderSummaryDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public double Total { get; set; }
        public OrderStatusEnum Status { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // For buyer view
        public Guid? ShopId { get; set; }
        public string? ShopName { get; set; }
        
        // For seller view
        public Guid? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        
        public int TotalItems { get; set; }
        public OrderReviewStatusEnum? ReviewStatus { get; set; }
    }
}
