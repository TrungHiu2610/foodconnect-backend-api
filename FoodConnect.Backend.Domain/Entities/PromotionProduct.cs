namespace FoodConnect.Backend.Domain.Entities
{
    public class PromotionProduct
    {
        public Guid PromotionId { get; set; }
        public virtual Promotion Promotion { get; set; } = null!;

        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;
    }
}
