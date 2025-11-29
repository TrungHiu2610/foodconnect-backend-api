namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;

public class ConversationResponse
{
    public Guid Id { get; set; }
    public Guid BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string? BuyerAvatar { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? SellerAvatar { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // For mapping - determine other user based on current user
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string? OtherUserAvatar { get; set; }
}
