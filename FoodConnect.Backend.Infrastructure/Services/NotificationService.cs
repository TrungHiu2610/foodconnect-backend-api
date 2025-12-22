using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Domain.Entities;
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

        public async Task SendWithdrawalNotificationAsync(Guid userId, NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .ReceiveWithdrawalNotification(notification);

                Console.WriteLine($"[NotificationService] Sent withdrawal notification to user {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error sending withdrawal notification: {ex.Message}");
            }
        }

        public async Task SendWithdrawalStatusUpdateAsync(Guid userId, Guid withdrawalId, string status, string message)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .ReceiveWithdrawalStatusUpdate(withdrawalId.ToString(), status, message);

                Console.WriteLine($"[NotificationService] Sent withdrawal status update to user {userId}: {status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error sending withdrawal status: {ex.Message}");
            }
        }

        public async Task NotifyAdminNewWithdrawalRequestAsync(Guid withdrawalId, string sellerName, decimal amount)
        {
            try
            {
                var notification = new NotificationDto
                {
                    Id = Guid.NewGuid(),
                    Title = WithdrawalNotificationMessages.ADMIN_NEW_REQUEST_TITLE,
                    Message = WithdrawalNotificationMessages.AdminNewRequestMessage(sellerName, amount),
                    Type = Domain.Enums.NotificationTypeEnum.WithdrawalRequest,
                    ReferenceId = withdrawalId.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    Priority = "high"
                };

                await _hubContext.Clients
                    .Group("admin")
                    .ReceiveWithdrawalNotification(notification);

                Console.WriteLine($"[NotificationService] Notified admins of new withdrawal request from {sellerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error notifying admin: {ex.Message}");
            }
        }

        public async Task NotifySellerWithdrawalProcessedAsync(Guid sellerId, Guid withdrawalId, bool isApproved, string message)
        {
            try
            {
                var notification = new NotificationDto
                {
                    Id = Guid.NewGuid(),
                    Title = isApproved ? WithdrawalNotificationMessages.SELLER_APPROVED_TITLE : WithdrawalNotificationMessages.SELLER_REJECTED_TITLE,
                    Message = message,
                    Type = isApproved ? Domain.Enums.NotificationTypeEnum.WithdrawalApproved : Domain.Enums.NotificationTypeEnum.WithdrawalRejected,
                    ReferenceId = withdrawalId.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    Priority = "high"
                };

                await SendWithdrawalNotificationAsync(sellerId, notification);
                await SendWithdrawalStatusUpdateAsync(
                    sellerId, 
                    withdrawalId, 
                    isApproved ? "Completed" : "Rejected", 
                    message
                );

                Console.WriteLine($"[NotificationService] Notified seller {sellerId} of withdrawal {(isApproved ? "approval" : "rejection")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error notifying seller: {ex.Message}");
            }
        }

        public async Task NotifySellerWithdrawalResolvedAsync(Guid sellerId, Guid withdrawalId, string message)
        {
            try
            {
                var notification = new NotificationDto
                {
                    Id = Guid.NewGuid(),
                    Title = WithdrawalNotificationMessages.SELLER_ISSUE_RESOLVED_TITLE,
                    Message = message,
                    Type = Domain.Enums.NotificationTypeEnum.WithdrawalResolved,
                    ReferenceId = withdrawalId.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    Priority = "normal"
                };

                await SendWithdrawalNotificationAsync(sellerId, notification);
                await SendWithdrawalStatusUpdateAsync(sellerId, withdrawalId, "Resolved", message);

                Console.WriteLine($"[NotificationService] Notified seller {sellerId} of withdrawal issue resolution");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Error notifying seller of resolution: {ex.Message}");
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
