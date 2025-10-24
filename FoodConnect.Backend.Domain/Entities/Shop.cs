using FoodConnect.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Entities
{
    public class Shop:BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal? Rating { get; set; }
        public ShopStatusEnum Status { get; set; }
        public string Street { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<ShopOperatingHour> OperatingHours { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
