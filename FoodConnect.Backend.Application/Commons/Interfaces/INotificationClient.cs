using FoodConnect.Backend.Application.Commons.DTOs.Notifications;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface INotificationClient
    {
        Task ReceiveOrderNotification(NotificationDto notification);
        Task ReceiveOrderStatusUpdate(string orderId, string status, string message);
        
        Task ReceiveNewOrderAlert(NotificationDto notification);
        
        Task ReceiveWithdrawalNotification(NotificationDto notification);
        Task ReceiveWithdrawalStatusUpdate(string withdrawalId, string status, string message);
        
        Task ReceiveNotification(NotificationDto notification);
        
        Task UpdateUnreadCount(int count);
        
        Task StopSoundAlert(string notificationId);
    }
}
