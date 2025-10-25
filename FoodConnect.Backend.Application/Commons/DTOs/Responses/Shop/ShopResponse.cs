using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop
{
    public class ShopResponse
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Address { get; set; } = string.Empty;
        public ShopStatusEnum Status { get; set; }
        public string? Description { get; set; }
        public string? AdminReason { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public PayoutMethodTypeEnum PayoutMethod { get; set; }
        public string PayoutAccountInfo { get; set; } = string.Empty;
        public string PayoutAccountName { get; set; } = string.Empty;
        public List<ShopAssetResponse> Assets { get; set; } = new List<ShopAssetResponse>();
        public List<ShopCategoryResponse> Categories { get; set; } = new List<ShopCategoryResponse>();
        public List<ShopOperatingHourResponse> OperatingHours { get; set; } = new List<ShopOperatingHourResponse>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ShopAssetResponse
    {
        public Guid Id { get; set; }
        public string AssetUrl { get; set; } = string.Empty;
        public ShopAssetTypeEnum AssetType { get; set; }
    }

    public class ShopCategoryResponse
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class ShopOperatingHourResponse
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
    }
}
