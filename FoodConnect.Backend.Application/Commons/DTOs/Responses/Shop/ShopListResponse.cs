namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop
{
    public class ShopListResponse
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Status { get; set; }  
        public string StatusName { get; set; } = string.Empty;  // "Active", "Draft"
        public DateTime CreatedAtUtc { get; set; }
    }
}
