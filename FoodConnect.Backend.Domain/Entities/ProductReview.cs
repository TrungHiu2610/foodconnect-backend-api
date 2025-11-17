using FoodConnect.Backend.Domain.Interfaces;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ProductReview : BaseEntity, IHardDelete
    {
        public int Rating { get; set; } // 1-5 stars
        public string? Comment { get; set; }
        
        // Relations
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        
        public Guid BuyerId { get; set; }
        public virtual User Buyer { get; set; } = null!;
        
        // Review assets (images/videos) - Max 5
        public virtual ICollection<ProductReviewAsset> Assets { get; set; } = new List<ProductReviewAsset>();
        
        // Response from seller (optional)
        public string? SellerResponse { get; set; }
        public DateTime? SellerRespondedAt { get; set; }
    }
}
