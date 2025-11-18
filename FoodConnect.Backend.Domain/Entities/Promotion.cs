using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities
{
    public class Promotion : BaseEntity
    {
        public Guid ShopId { get; set; }
        public virtual Shop Shop { get; set; } = null!;

        public string PromotionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }

        public PromotionTypeEnum PromotionType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderValue { get; set; }

        public int? MaxUsageCount { get; set; }
        public int UsagePerCustomer { get; set; } = 1;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public PromotionStatusEnum Status { get; set; } = PromotionStatusEnum.Draft;
        public bool ApplicableToAllProducts { get; set; } = false;

        public int TotalUsedCount { get; set; } = 0;

        public string? AdminReason { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();
        public virtual ICollection<PromotionUsage> PromotionUsages { get; set; } = new List<PromotionUsage>();
    }
}
