using FoodConnect.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Entities
{
    public class Shop : BaseEntity
    {
        public string ShopName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal? Rating { get; set; }
        public int ReviewCount { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
        public bool IsPromoted { get; set; } = false;
        public DateTime? LastOrderAt { get; set; }
        public bool IsVerified { get; set; } = false;
        public ShopStatusEnum Status { get; set; }
        
        public string SellerFullName { get; set; } = string.Empty;
        public string SellerEmail { get; set; } = string.Empty;
        public string SellerPhone { get; set; } = string.Empty;
        
        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        public string? AdminReason { get; set; }  
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        
        public PaymentMethodEnum PayoutMethod { get; set; }
        public string PayoutAccountInfo { get; set; } = string.Empty;
        public string PayoutAccountName { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<ShopOperatingHour> OperatingHours { get; set; } = new List<ShopOperatingHour>();
        public virtual ICollection<ShopAsset> Assets { get; set; } = new List<ShopAsset>();
        public virtual ICollection<ShopCategory> ShopCategories { get; set; } = new List<ShopCategory>();

        public double? CalculatedDistance { get; set; }

        public string GetFullAddress()
        {
            return string.Join(", ", new[] { Street, Ward, District, City, Country }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        public bool IsOpenNow()
        {
            if (!OperatingHours.Any()) return true; // If no hours set, assume always open

            var now = DateTime.Now;
            var dayOfWeek = now.DayOfWeek;
            var currentTime = TimeOnly.FromDateTime(now);

            var todayHours = OperatingHours.FirstOrDefault(h => h.DayOfWeek == dayOfWeek);
            if (todayHours == null) return false;

            return currentTime >= todayHours.OpenTime && currentTime <= todayHours.CloseTime;
        }
    }
}
