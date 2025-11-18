using FoodConnect.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderCode { get; set; } = string.Empty;
        public double SubTotal { get; set; }
        public double ShippingFee { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public OrderStatusEnum Status { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public DeliveryTypeEnum DeliveryType { get; set; } = DeliveryTypeEnum.Standard;
        public double? DistanceKm { get; set; }
        public string ShippingAddressJson { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CancelReason { get; set; }

        public Guid? PromotionId { get; set; }
        public virtual Promotion? Promotion { get; set; }
        public decimal? PromotionDiscountAmount { get; set; }
        public string? PromotionCode { get; set; }
        
        public string? DeliveryProofImageUrl { get; set; }
        
        public DateTime? AcceptedAt { get; set; }
        public DateTime? PreparedAt { get; set; }
        public DateTime? ReadyForPickupAt { get; set; }
        public DateTime? DeliveryStartedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public Guid BuyerId { get; set; }
        public User Buyer { get; set; } = null!;
        public Guid ShopId { get; set; }
        public Shop Shop { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    }
}
