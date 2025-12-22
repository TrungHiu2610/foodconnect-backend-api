using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities;

public class AIChatMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public AIChatMessageRoleEnum Role { get; set; }
    public string Content { get; set; } = string.Empty;
    
    public string? RecommendedProductIds { get; set; }
    public string? IntentJson { get; set; }
    public string? RetrievalMetadata { get; set; }
    
    public virtual AIChatConversation Conversation { get; set; } = null!;
}
