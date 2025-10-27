using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Domain.Interfaces;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ProductAsset : BaseEntity, IHardDelete
    {
        public string AssetName { get; set; }
        public string? AssetDescription { get; set; }
        public string AssetUrl { get; set; }
        public ProductAssetTypeEnum AssetType { get; set; } 
        public bool IsThumbnail { get; set; } 

        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
