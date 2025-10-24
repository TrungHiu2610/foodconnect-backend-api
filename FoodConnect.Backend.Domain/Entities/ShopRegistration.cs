using FoodConnect.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ShopRegistration : BaseEntity
    {
        public Guid UserId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string ShopDescription { get; set; } = string.Empty;
        public ShopRegistrationStatusEnum Status { get; set; }
        public string? AdminReason { get; set; }
        public Guid? ReviewedBy { get; set; }
        public PayoutMethodTypeEnum PayoutMethod { get; set; }
        public string PayoutAccountInfo { get; set; } = string.Empty;
        public string PayoutAccountName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Shop? Shop { get; set; }
        public virtual ICollection<ShopRegistrationCategory> ShopRegistrationCategories { get; set; } = new List<ShopRegistrationCategory>();
        public virtual ICollection<ShopOperatingHour> OperatingHours { get; set; } = new List<ShopOperatingHour>();
        public virtual ICollection<ShopRegistrationAsset> Assets { get; set; } = new List<ShopRegistrationAsset>();
    }
}
