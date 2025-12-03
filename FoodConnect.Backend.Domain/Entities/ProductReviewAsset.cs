using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Domain.Interfaces;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ProductReviewAsset : BaseEntity, IHardDelete
    {
        public string AssetUrl { get; set; } = string.Empty;
        public ProductAssetTypeEnum AssetType { get; set; }
        public int DisplayOrder { get; set; }
        
        public Guid ProductReviewId { get; set; }
        public ProductReview ProductReview { get; set; } = null!;
    }
}
