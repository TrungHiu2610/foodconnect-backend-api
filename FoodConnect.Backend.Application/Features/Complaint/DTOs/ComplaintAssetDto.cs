using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Complaint.DTOs
{
    public class ComplaintAssetDto
    {
        public Guid Id { get; set; }
        public string AssetUrl { get; set; } = string.Empty;
        public OrderComplaintAssetTypeEnum AssetType { get; set; }
        public string AssetTypeName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
