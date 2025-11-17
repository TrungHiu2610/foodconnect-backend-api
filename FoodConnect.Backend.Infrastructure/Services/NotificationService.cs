using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FoodConnect.Backend.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

        public NotificationService(IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(Guid userId, NotificationDto notification)
        {
            try
            {
                // Send to user's group
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .ReceiveNotification(notification);

                Console.WriteLine($"[NotificationService] Sent notification to user {userId}: {notification.Title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error sending to user {userId}: {ex.Message}");
            }
        }

        public async Task SendOrderStatusUpdateAsync(Guid userId, Guid orderId, string status, string message)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .ReceiveOrderStatusUpdate(orderId.ToString(), status, message);

                Console.WriteLine($"[NotificationService] Sent order status update to user {userId}: Order {orderId} - {status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error sending order status: {ex.Message}");
            }
        }

        public async Task SendNewOrderAlertAsync(Guid shopOwnerId, NotificationDto notification)
        {
            try
            {
                // Send to shop owner's group
                await _hubContext.Clients
                    .Group($"user_{shopOwnerId}")
                    .ReceiveNewOrderAlert(notification);

                Console.WriteLine($"[NotificationService] Sent new order alert to shop owner {shopOwnerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error sending new order alert: {ex.Message}");
            }
        }

        public async Task SendToGroupAsync(string groupName, NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients
                    .Group(groupName)
                    .ReceiveNotification(notification);

                Console.WriteLine($"[NotificationService] Sent notification to group {groupName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error sending to group: {ex.Message}");
            }
        }

        public async Task UpdateUnreadCountAsync(Guid userId, int count)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .UpdateUnreadCount(count);

                Console.WriteLine($"[NotificationService] Updated unread count for user {userId}: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error updating unread count: {ex.Message}");
            }
        }

        public async Task BroadcastAsync(NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.All.ReceiveNotification(notification);
                Console.WriteLine($"[NotificationService] Broadcasted notification: {notification.Title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error broadcasting: {ex.Message}");
            }
        }

        public async Task StopSoundAlertAsync(Guid userId, Guid notificationId)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .StopSoundAlert(notificationId.ToString());

                Console.WriteLine($"[NotificationService] Sent stop sound alert to user {userId} for notification {notificationId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error stopping sound alert: {ex.Message}");
            }
        }
    }
}
