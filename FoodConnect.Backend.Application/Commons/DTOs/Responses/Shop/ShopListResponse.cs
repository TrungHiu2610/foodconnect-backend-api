using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop
{
    public class ShopListResponse
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public ShopStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
