using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.DTOs.Notifications
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public NotificationTypeEnum Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        
        // Optional data
        public Guid? OrderId { get; set; }
        public string? OrderCode { get; set; }
        public Guid? ShopId { get; set; }
        public string? ShopName { get; set; }
        public string? MetadataJson { get; set; }
        public bool RequiresSound { get; set; }
        public int? SoundRepeatIntervalSeconds { get; set; }
        public bool RequiresAction { get; set; }
    }
    
    public class CreateNotificationDto
    {
        public Guid UserId { get; set; }
        public NotificationTypeEnum Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public Guid? ShopId { get; set; }
        public string? MetadataJson { get; set; }
    }
    
    public class MarkNotificationAsReadDto
    {
        public List<Guid> NotificationIds { get; set; } = new List<Guid>();
    }
    
    public class NotificationSummaryDto
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public List<NotificationDto> Recent { get; set; } = new List<NotificationDto>();
    }
    
    public class NotificationActionResult
    {
        public bool Success { get; set; }
        public int AffectedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
