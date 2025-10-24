using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ShopRegistrationCategory : BaseEntity
    {
        public Guid ShopRegistrationId { get; set; }
        public Guid CategoryId { get; set; }
        
        public virtual ShopRegistration ShopRegistration { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
    }
}
