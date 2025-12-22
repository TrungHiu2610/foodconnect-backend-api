namespace FoodConnect.Backend.Domain.Entities;

public class AIChatConversation : BaseEntity
{
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public DateTime? LastMessageAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual User User { get; set; } = null!;
    public virtual ICollection<AIChatMessage> Messages { get; set; } = new List<AIChatMessage>();
}
