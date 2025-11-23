using FoodConnect.Backend.Application.Commons.DTOs.Notifications;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface INotificationClient
    {
        // Order notifications
        Task ReceiveOrderNotification(NotificationDto notification);
        Task ReceiveOrderStatusUpdate(string orderId, string status, string message);
        
        // Shop notifications
        Task ReceiveNewOrderAlert(NotificationDto notification);
        
        // Withdrawal notifications
        Task ReceiveWithdrawalNotification(NotificationDto notification);
        Task ReceiveWithdrawalStatusUpdate(string withdrawalId, string status, string message);
        
        // Generic notification
        Task ReceiveNotification(NotificationDto notification);
        
        // Badge count update
        Task UpdateUnreadCount(int count);
        
        Task StopSoundAlert(string notificationId);
    }
}
