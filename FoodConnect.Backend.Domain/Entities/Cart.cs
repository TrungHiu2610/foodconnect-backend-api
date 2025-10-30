using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public Guid? UserId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public virtual User? User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
