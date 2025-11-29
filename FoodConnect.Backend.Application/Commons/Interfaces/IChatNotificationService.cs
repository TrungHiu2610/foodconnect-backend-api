using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;

namespace FoodConnect.Backend.Application.Commons.Interfaces;

/// <summary>
/// Service to send real-time chat messages via SignalR
/// </summary>
public interface IChatNotificationService
{
    /// <summary>
    /// Send message to conversation room
    /// </summary>
    Task SendMessageToConversationAsync(string conversationId, MessageResponse message);

    /// <summary>
    /// Send typing indicator to conversation
    /// </summary>
    Task SendTypingIndicatorAsync(string conversationId, string userId);

    /// <summary>
    /// Send stop typing indicator
    /// </summary>
    Task SendStopTypingIndicatorAsync(string conversationId, string userId);

    /// <summary>
    /// Send read receipt
    /// </summary>
    Task SendReadReceiptAsync(string conversationId, string userId, List<string> messageIds);

    /// <summary>
    /// Notify user about new message
    /// </summary>
    Task NotifyNewMessageAsync(Guid conversationId, Guid messageId, Guid recipientUserId, string messagePreview);
}
