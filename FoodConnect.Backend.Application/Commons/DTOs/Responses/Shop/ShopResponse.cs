namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop
{
    public class ShopResponse
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string Address { get; set; } = string.Empty;
        public int Status { get; set; }  
        public string StatusName { get; set; } = string.Empty;  // "Active", "Draft", etc.
        public string? AdminReason { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int PayoutMethod { get; set; }  
        public string PayoutMethodName { get; set; } = string.Empty;  // "Bank", "MoMo"
        public string PayoutAccountInfo { get; set; } = string.Empty;
        public string PayoutAccountName { get; set; } = string.Empty;
        
        // Seller contact info (from Shop entity, not User)
        public string SellerFullName { get; set; } = string.Empty;
        public string SellerEmail { get; set; } = string.Empty;
        public string SellerPhone { get; set; } = string.Empty;
        
        // User info (Shop owner from User entity)
        public string OwnerName { get; set; } = string.Empty;  // User.FullName
        public string Phone { get; set; } = string.Empty;  // User.PhoneNumber
        public string? Email { get; set; }  // User.Email
        
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
        public int AssetType { get; set; } 
        public string AssetTypeName { get; set; } = string.Empty;  // "IdCardFront", etc.
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
