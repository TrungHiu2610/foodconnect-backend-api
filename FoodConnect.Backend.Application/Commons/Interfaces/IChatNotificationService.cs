using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;

namespace FoodConnect.Backend.Application.Commons.Interfaces;

public interface IChatNotificationService
{
    Task SendMessageToConversationAsync(string conversationId, MessageResponse message);
    Task SendTypingIndicatorAsync(string conversationId, string userId);
    Task SendStopTypingIndicatorAsync(string conversationId, string userId);
    Task SendReadReceiptAsync(string conversationId, string userId, List<string> messageIds);
    Task NotifyNewMessageAsync(Guid conversationId, Guid messageId, Guid recipientUserId, string messagePreview);
    Task UpdateUnreadCountAsync(Guid userId, int unreadCount);
}
