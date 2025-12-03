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
        public string EstimatedDelivery { get; set; } = string.Empty;
        
        public Guid? ShopId { get; set; }
        public string? ShopName { get; set; }
        
        public Guid? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public int TotalItems { get; set; }
        public FirstProductDto? FirstProduct { get; set; }
        public OrderReviewStatusEnum? ReviewStatus { get; set; }
    }
    
    public class FirstProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }
    }
}
