namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion
{
    public class PromotionDetailForBuyerResponse
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? ShopLogoUrl { get; set; }
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
        public bool ApplicableToAllProducts { get; set; }
        public int RemainingUsage { get; set; }
        public bool CanUse { get; set; }
        public int UserUsageCount { get; set; }
        public List<PromotionProductForBuyerDto> ApplicableProducts { get; set; } = new();
    }

    public class PromotionProductForBuyerDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsAvailable { get; set; }
        public int? StockQuantity { get; set; }
    }
}
