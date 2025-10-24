using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.ShopRegistration
{
    public class ShopRegistrationResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string ShopDescription { get; set; } = string.Empty;
        public ShopRegistrationStatusEnum Status { get; set; }
        public string? AdminReason { get; set; }
        public Guid? ReviewedBy { get; set; }
        public string? ReviewedByFullName { get; set; }
        public PayoutMethodTypeEnum PayoutMethod { get; set; }
        public string PayoutAccountInfo { get; set; } = string.Empty;
        public string PayoutAccountName { get; set; } = string.Empty;
        
        // Address fields
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public List<ShopRegistrationAssetResponse> Assets { get; set; } = new List<ShopRegistrationAssetResponse>();
        public List<ShopRegistrationCategoryResponse> Categories { get; set; } = new List<ShopRegistrationCategoryResponse>();
        public List<ShopRegistrationOperatingHourResponse> OperatingHours { get; set; } = new List<ShopRegistrationOperatingHourResponse>();
    }

    public class ShopRegistrationAssetResponse
    {
        public Guid Id { get; set; }
        public string AssetUrl { get; set; } = string.Empty;
        public ShopRegistrationAssetTypeEnum AssetType { get; set; }
    }

    public class ShopRegistrationCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ShopRegistrationOperatingHourResponse
    {
        public Guid Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
    }
}
