using FoodConnect.Backend.Domain.Interfaces;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ShopCategory : BaseEntity, IHardDelete
    {
        public Guid ShopId { get; set; }
        public Guid CategoryId { get; set; }
        
        public virtual Shop Shop { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
    }
}
