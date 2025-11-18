namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion
{
    public class PromotionListResponse
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string PromotionName { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public int PromotionType { get; set; }
        public string PromotionTypeName { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int TotalUsedCount { get; set; }
        public int? MaxUsageCount { get; set; }
        public int RemainingUsage { get; set; }
        public bool ApplicableToAllProducts { get; set; }
        public int ProductCount { get; set; }
    }
}
