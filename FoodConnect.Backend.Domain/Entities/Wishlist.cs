using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities
{
    public class Wishlist : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public Guid? ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public Guid? ShopId { get; set; }
        public virtual Shop? Shop { get; set; }

        public WishlistTypeEnum Type { get; set; }
        public bool NotificationEnabled { get; set; } = true;
    }
}
