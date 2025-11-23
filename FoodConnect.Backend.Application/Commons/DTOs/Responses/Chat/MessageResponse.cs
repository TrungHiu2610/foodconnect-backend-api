namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;

public class MessageResponse
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatar { get; set; }
    public int MessageType { get; set; }
    public string MessageTypeName { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
