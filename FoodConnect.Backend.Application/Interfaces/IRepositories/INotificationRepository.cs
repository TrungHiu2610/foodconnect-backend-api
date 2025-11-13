using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int limit = 20, int offset = 0);
        Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<Notification?> GetByIdWithDetailsAsync(Guid id);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkMultipleAsReadAsync(List<Guid> notificationIds);
        Task MarkAllAsReadAsync(Guid userId);
        Task<List<Notification>> GetNotificationsByTypeAsync(Guid userId, NotificationTypeEnum type);
        Task DeleteOldNotificationsAsync(DateTime olderThan);
        Task<Notification?> GetNotificationByOrderIdAsync(Guid orderId, Guid userId, NotificationTypeEnum type);
    }
}
