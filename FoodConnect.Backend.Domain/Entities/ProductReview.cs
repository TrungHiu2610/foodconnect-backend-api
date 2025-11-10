namespace FoodConnect.Backend.Domain.Entities
{
    public class ProductReview : BaseEntity
    {
        public int Rating { get; set; } // 1-5 stars
        public string? Comment { get; set; }
        public string? ReviewImageUrl { get; set; }
        
        // Relations
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;
        
        public Guid BuyerId { get; set; }
        public User Buyer { get; set; } = null!;
        
        // Response from seller (optional)
        public string? SellerResponse { get; set; }
        public DateTime? SellerRespondedAt { get; set; }
    }
}
