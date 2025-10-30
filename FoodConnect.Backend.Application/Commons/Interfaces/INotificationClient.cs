using FoodConnect.Backend.Application.Commons.DTOs.Notifications;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    /// <summary>
    /// Defines methods that can be called by server on SignalR clients
    /// Client must implement these methods to receive notifications
    /// </summary>
    public interface INotificationClient
    {
        // Order notifications
        Task ReceiveOrderNotification(NotificationDto notification);
        Task ReceiveOrderStatusUpdate(string orderId, string status, string message);
        
        // Shop notifications
        Task ReceiveNewOrderAlert(NotificationDto notification);
        
        // Generic notification
        Task ReceiveNotification(NotificationDto notification);
        
        // Badge count update
        Task UpdateUnreadCount(int count);
    }
}
