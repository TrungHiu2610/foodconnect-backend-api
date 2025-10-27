using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Domain.Interfaces;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ShopAsset : BaseEntity, IHardDelete
    {
        public Guid ShopId { get; set; }
        public string AssetUrl { get; set; } = string.Empty;
        public ShopAssetTypeEnum AssetType { get; set; }
        
        public virtual Shop Shop { get; set; } = null!;
    }
}
