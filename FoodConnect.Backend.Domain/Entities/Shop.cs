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
        // Basic shop info
        public string ShopName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal? Rating { get; set; }
        public ShopStatusEnum Status { get; set; }
        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        // Approval metadata
        public string? AdminReason { get; set; }  // Reject/Suspend reason
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        
        // Payout information
        public PayoutMethodTypeEnum PayoutMethod { get; set; }
        public string PayoutAccountInfo { get; set; } = string.Empty;
        public string PayoutAccountName { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<ShopOperatingHour> OperatingHours { get; set; } = new List<ShopOperatingHour>();
        public virtual ICollection<ShopAsset> Assets { get; set; } = new List<ShopAsset>();
        public virtual ICollection<ShopCategory> ShopCategories { get; set; } = new List<ShopCategory>();
    }
}
