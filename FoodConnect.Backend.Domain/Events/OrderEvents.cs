using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Events
{
    public abstract class BaseOrderEvent
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public double Total { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
    
    public class OrderCreatedEvent : BaseOrderEvent
    {
        public PaymentMethodEnum PaymentMethod { get; set; }
        public int ItemCount { get; set; }
    }
    
    public class OrderStatusChangedEvent : BaseOrderEvent
    {
        public OrderStatusEnum OldStatus { get; set; }
        public OrderStatusEnum NewStatus { get; set; }
        public string? Reason { get; set; } // For rejection/cancellation
    }
    
    public class OrderCancelledEvent : BaseOrderEvent
    {
        public string? CancelReason { get; set; }
        public bool IsBuyerCancelled { get; set; }
    }
    
    public class OrderRejectedEvent : BaseOrderEvent
    {
        public string RejectionReason { get; set; } = string.Empty;
    }
}
