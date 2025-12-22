namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop
{
    public class ShopDetailForBuyerResponse
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Distance { get; set; } // km from user location (if provided)
        
        public string PhoneNumber { get; set; } = string.Empty;
        
        public decimal? Rating { get; set; }
        public int ReviewCount { get; set; }
        public int TotalOrders { get; set; }
        
        public bool IsOpen { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsVerified { get; set; }
        public List<string> Badges { get; set; } = new();
        
        public List<string> Categories { get; set; } = new();
        
        public List<OperatingHourDto> OperatingHours { get; set; } = new();
        
        public List<string> AdditionalImages { get; set; } = new();
        
        public List<ShopProductDto> AvailableProducts { get; set; } = new();
    }

    public class OperatingHourDto
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
    }

    public class ShopProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; }
        public bool IsAvailable { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        
        public string DeliveryType { get; set; } = string.Empty; // "Express" or "Standard"
        public bool IsDeliverable { get; set; } = true; 
        public string? DeliverabilityMessage { get; set; } 
        public List<string> ProductBadges { get; set; } = new(); //["Ngoài vùng giao hàng"]
    }
}
