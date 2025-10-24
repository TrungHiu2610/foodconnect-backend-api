using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.ShopRegistration
{
    public class ShopRegistrationListResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public ShopRegistrationStatusEnum Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
