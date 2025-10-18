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
        public string? CoverImageUrl { get; set; }
        public ShopStatusEnum Status { get; set; } = ShopStatusEnum.PendingApproval;
        public decimal? Rating { get; set; }

        public Guid userId { get; set; }
        public virtual User User { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
