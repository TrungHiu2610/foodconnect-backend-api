using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public string? VnpayData { get; set; }
    public TransactionStatusEnum Status { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    
    // For multi-order payments (VNPay): JSON array of order IDs
    // Example: ["guid1", "guid2", "guid3"]
    public string? OrderIds { get; set; }
    
    public virtual Order Order { get; set; } = null!;
    public virtual User Buyer { get; set; } = null!;
}
