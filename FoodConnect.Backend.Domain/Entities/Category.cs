using FoodConnect.Backend.Domain.Enums;
namespace FoodConnect.Backend.Domain.Entities
{
    public class Category:BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DeliveryTypeEnum DeliveryType { get; set; }

        public Guid? ParentId { get; set; }
        public virtual Category? Parent { get; set; }
        public ICollection<Product>? Products { get; set; } = new List<Product>();
    }
}
