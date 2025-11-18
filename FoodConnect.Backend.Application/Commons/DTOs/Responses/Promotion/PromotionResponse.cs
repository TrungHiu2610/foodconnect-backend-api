namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion
{
    public class PromotionResponse
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string PromotionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public int PromotionType { get; set; }
        public string PromotionTypeName { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderValue { get; set; }
        public int? MaxUsageCount { get; set; }
        public int UsagePerCustomer { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool ApplicableToAllProducts { get; set; }
        public int TotalUsedCount { get; set; }
        public int RemainingUsage { get; set; }
        public string? AdminReason { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PromotionProductDto> ApplicableProducts { get; set; } = new();
    }

    public class PromotionProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
