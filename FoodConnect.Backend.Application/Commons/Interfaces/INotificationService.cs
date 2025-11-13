using FoodConnect.Backend.Application.Commons.DTOs.Notifications;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface INotificationService
    {
        // Send to specific user
        Task SendToUserAsync(Guid userId, NotificationDto notification);
        Task SendOrderStatusUpdateAsync(Guid userId, Guid orderId, string status, string message);
        
        // Send to shop owner
        Task SendNewOrderAlertAsync(Guid shopOwnerId, NotificationDto notification);
        
        // Send to group (e.g., all admins)
        Task SendToGroupAsync(string groupName, NotificationDto notification);
        
        // Update badge count
        Task UpdateUnreadCountAsync(Guid userId, int count);
        
        // Broadcast to all connected users
        Task BroadcastAsync(NotificationDto notification);
        
        // Stop sound alert for specific notification
        Task StopSoundAlertAsync(Guid userId, Guid notificationId);
    }
}
