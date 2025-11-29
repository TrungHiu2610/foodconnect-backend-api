using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public MessageTypeEnum MessageType { get; set; }
    public string? Content { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsRead { get; set; } = false;
    
    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
}
