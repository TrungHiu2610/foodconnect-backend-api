using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FoodConnect.Backend.Infrastructure.Services;

public class ChatNotificationService : IChatNotificationService
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public ChatNotificationService(IHubContext<ChatHub, IChatClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendMessageToConversationAsync(string conversationId, MessageResponse message)
    {
        await _hubContext.Clients
            .Group($"conversation_{conversationId}")
            .ReceiveMessage(message);
    }

    public async Task SendTypingIndicatorAsync(string conversationId, string userId)
    {
        await _hubContext.Clients
            .Group($"conversation_{conversationId}")
            .ReceiveTypingIndicator(userId);
    }

    public async Task SendStopTypingIndicatorAsync(string conversationId, string userId)
    {
        await _hubContext.Clients
            .Group($"conversation_{conversationId}")
            .ReceiveStopTypingIndicator(userId);
    }

    public async Task SendReadReceiptAsync(string conversationId, string userId, List<string> messageIds)
    {
        await _hubContext.Clients
            .Group($"conversation_{conversationId}")
            .ReceiveReadReceipt(userId, messageIds);
    }

    public async Task NotifyNewMessageAsync(Guid conversationId, Guid messageId, Guid recipientUserId, string messagePreview)
    {
        await _hubContext.Clients
            .Group($"user_{recipientUserId}")
            .ReceiveError($"New message: {messagePreview}");
        
        Console.WriteLine($"[ChatNotificationService] Notified user {recipientUserId} about message in conversation {conversationId}");
    }

    public async Task UpdateUnreadCountAsync(Guid userId, int unreadCount)
    {
        await _hubContext.Clients
            .Group($"user_{userId}")
            .UpdateUnreadCount(unreadCount);
        
        Console.WriteLine($"[ChatNotificationService] Updated unread count for user {userId}: {unreadCount}");
    }
}
