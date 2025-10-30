using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public NotificationTypeEnum Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        
        // Optional: Link to related entity
        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }
        
        public Guid? ShopId { get; set; }
        public Shop? Shop { get; set; }
        
        // Metadata as JSON
        public string? MetadataJson { get; set; }
        
        // Status
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        
        // For future: Push notification tracking
        public bool IsPushed { get; set; } = false;
        public DateTime? PushedAt { get; set; }
    }
}
