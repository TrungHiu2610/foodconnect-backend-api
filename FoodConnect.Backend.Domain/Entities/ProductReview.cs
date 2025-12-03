using FoodConnect.Backend.Domain.Interfaces;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ProductReview : BaseEntity, IHardDelete
    {
        public int Rating { get; set; } // 1-5 stars
        public string? Comment { get; set; }
        
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        
        public Guid BuyerId { get; set; }
        public virtual User Buyer { get; set; } = null!;
        
        public virtual ICollection<ProductReviewAsset> Assets { get; set; } = new List<ProductReviewAsset>();
        
        public string? SellerResponse { get; set; }
        public DateTime? SellerRespondedAt { get; set; }
    }
}
