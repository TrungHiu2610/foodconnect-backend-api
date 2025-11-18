namespace FoodConnect.Backend.Domain.Entities
{
    public class PromotionUsage : BaseEntity
    {
        public Guid PromotionId { get; set; }
        public virtual Promotion Promotion { get; set; } = null!;

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public decimal DiscountAmount { get; set; }
        public decimal OrderValue { get; set; }
        public DateTime UsedAt { get; set; }
    }
}
