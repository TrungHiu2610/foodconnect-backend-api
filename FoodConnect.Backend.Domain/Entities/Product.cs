using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public ProductStatusEnum Status { get; set; } = ProductStatusEnum.Draft;

        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public Guid ShopId { get; set; }
        public virtual Shop Shop { get; set; }
        public ICollection<ProductAsset> ProductAssets { get; set; } = new List<ProductAsset>();
        public ICollection<ProductDailyAvailability> ProductDailyAvailabilities { get; set; } = new List<ProductDailyAvailability>();
    }
}
