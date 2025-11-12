namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Product
{
    public class ProductReviewResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        
        // Buyer info
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string? BuyerAvatarUrl { get; set; }
        
        // Review content
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<ProductReviewAssetDto> Assets { get; set; } = new();
        
        // Seller response
        public string? SellerResponse { get; set; }
        public DateTime? SellerRespondedAt { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
    }
}
