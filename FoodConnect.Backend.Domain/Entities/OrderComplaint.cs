using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Entities
{
    public class OrderComplaint : BaseEntity, IHardDelete
    {
        public string BuyerReason { get; set; } = string.Empty;
        public string? SellerResponse { get; set; }
        public bool IsFixedAmount { get; set; } = false;
        public decimal? SellerDesiredRefundAmount { get; set; }
        public string? AdminDecisionReason { get; set; }
        public decimal? ApprovedRefundAmount { get; set; }
        public ComplaintStatusEnum Status { get; set; } = ComplaintStatusEnum.Pending;
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        public Guid BuyerId { get; set; }
        public virtual User Buyer { get; set; } = null!;
        public Guid SellerId { get; set; }
        public virtual User Seller { get; set; } = null!;
        public Guid? AdminId { get; set; }
        public virtual User Admin { get; set; } = null!;
        public virtual ICollection<OrderComplaintAsset> Assets { get; set; } = new List<OrderComplaintAsset>();
    }
}
