namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Product
{
    public class ProductReviewResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string? BuyerAvatarUrl { get; set; }
        
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<ProductReviewAssetDto> Assets { get; set; } = new();
        
        public string? SellerResponse { get; set; }
        public DateTime? SellerRespondedAt { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
