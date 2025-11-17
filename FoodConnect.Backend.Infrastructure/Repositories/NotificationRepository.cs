using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int limit = 20, int offset = 0)
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .Include(n => n.Shop)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAtUtc)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId)
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .Include(n => n.Shop)
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<Notification?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .Include(n => n.Shop)
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                Update(notification);
            }
        }

        public async Task MarkMultipleAsReadAsync(List<Guid> notificationIds)
        {
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.Id) && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }
        }

        public async Task<List<Notification>> GetNotificationsByTypeAsync(Guid userId, NotificationTypeEnum type)
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .Include(n => n.Shop)
                .Where(n => n.UserId == userId && n.Type == type)
                .OrderByDescending(n => n.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task DeleteOldNotificationsAsync(DateTime olderThan)
        {
            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAtUtc < olderThan && n.IsRead)
                .ToListAsync();

            _context.Notifications.RemoveRange(oldNotifications);
        }

        public async Task<Notification?> GetNotificationByOrderIdAsync(Guid orderId, Guid userId, NotificationTypeEnum type)
        {
            return await _context.Notifications
                .Where(n => n.OrderId == orderId && n.UserId == userId && n.Type == type)
                .OrderByDescending(n => n.CreatedAtUtc)
                .FirstOrDefaultAsync();
        }
    }
}
