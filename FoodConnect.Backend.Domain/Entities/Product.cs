using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Ingredients { get; set; }
        public string Weight { get; set; }
        public string ExpiryDate { get; set; }
        public string StorageInstructions { get; set; }
        public string UsageInstructions { get; set; }
        public ProductStatusEnum Status { get; set; } = ProductStatusEnum.Draft;
        public bool IsAvailable { get; set; } = true;
        public int? StockQuantity { get; set; }

        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public Guid ShopId { get; set; }
        public virtual Shop Shop { get; set; }
        public ICollection<ProductAsset> ProductAssets { get; set; } = new List<ProductAsset>();
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
        public double? CalculatedDistance { get; set; }
    }
}
