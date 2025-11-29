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
        
        // For buyer view
        public Guid? ShopId { get; set; }
        public string? ShopName { get; set; }
        
        // For seller view
        public Guid? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        
        /// <summary>
        /// Total number of unique products (not total quantity)
        /// </summary>
        public int TotalItems { get; set; }
        
        /// <summary>
        /// First product information for preview
        /// </summary>
        public FirstProductDto? FirstProduct { get; set; }
        
        /// <summary>
        /// Review status for completed orders (only applicable when Status = Completed)
        /// </summary>
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
