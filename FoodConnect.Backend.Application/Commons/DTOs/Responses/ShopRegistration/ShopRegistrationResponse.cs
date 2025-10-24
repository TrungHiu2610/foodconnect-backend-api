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
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public List<ShopRegistrationAssetResponse> Assets { get; set; } = new List<ShopRegistrationAssetResponse>();
    }

    public class ShopRegistrationAssetResponse
    {
        public Guid Id { get; set; }
        public string AssetUrl { get; set; } = string.Empty;
        public ShopRegistrationAssetTypeEnum AssetType { get; set; }
    }
}
