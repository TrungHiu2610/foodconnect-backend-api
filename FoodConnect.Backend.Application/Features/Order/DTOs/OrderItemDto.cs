namespace FoodConnect.Backend.Application.Features.Order.DTOs
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        
        /// <summary>
        /// Indicates if this product has been reviewed by the buyer
        /// </summary>
        public bool IsReviewed { get; set; }
    }
}
