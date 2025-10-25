namespace FoodConnect.Backend.Domain.Entities
{
    public class ShopCategory : BaseEntity
    {
        public Guid ShopId { get; set; }
        public Guid CategoryId { get; set; }
        
        public virtual Shop Shop { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
    }
}
