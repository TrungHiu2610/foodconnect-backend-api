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
        
        // Withdrawal notifications
        Task SendWithdrawalNotificationAsync(Guid userId, NotificationDto notification);
        Task SendWithdrawalStatusUpdateAsync(Guid userId, Guid withdrawalId, string status, string message);
        Task NotifyAdminNewWithdrawalRequestAsync(Guid withdrawalId, string sellerName, decimal amount);
        Task NotifySellerWithdrawalProcessedAsync(Guid sellerId, Guid withdrawalId, bool isApproved, string message);
        Task NotifySellerWithdrawalResolvedAsync(Guid sellerId, Guid withdrawalId, string message);
        
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
