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
        public string ShopName { get; set; }
        public string ShopDescription { get; set; }
        public ShopRegistrationStatusEnum Status { get; set; }
        public string? AdminReason { get; set; }
        public Guid? ReviewedBy { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<ShopRegistrationAsset> Assets { get; set; }
    }
}
