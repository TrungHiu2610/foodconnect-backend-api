using FoodConnect.Backend.Application.Commons.DTOs.Notifications;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface INotificationService
    {
        Task SendToUserAsync(Guid userId, NotificationDto notification);
        Task SendOrderStatusUpdateAsync(Guid userId, Guid orderId, string status, string message);
        
        Task SendNewOrderAlertAsync(Guid shopOwnerId, NotificationDto notification);
        
        Task SendWithdrawalNotificationAsync(Guid userId, NotificationDto notification);
        Task SendWithdrawalStatusUpdateAsync(Guid userId, Guid withdrawalId, string status, string message);
        Task NotifyAdminNewWithdrawalRequestAsync(Guid withdrawalId, string sellerName, decimal amount);
        Task NotifySellerWithdrawalProcessedAsync(Guid sellerId, Guid withdrawalId, bool isApproved, string message);
        Task NotifySellerWithdrawalResolvedAsync(Guid sellerId, Guid withdrawalId, string message);
        
        Task SendToGroupAsync(string groupName, NotificationDto notification);
        
        Task UpdateUnreadCountAsync(Guid userId, int count);
        
        Task BroadcastAsync(NotificationDto notification);
        
        Task StopSoundAlertAsync(Guid userId, Guid notificationId);
    }
}
