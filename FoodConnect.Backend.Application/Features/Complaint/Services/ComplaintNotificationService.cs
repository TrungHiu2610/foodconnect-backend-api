using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using System.Text.Json;

namespace FoodConnect.Backend.Application.Features.Complaint.Services
{
    public class ComplaintNotificationService
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ComplaintNotificationService(
            INotificationService notificationService,
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task NotifyComplaintCreatedAsync(OrderComplaint complaint, CancellationToken cancellationToken = default)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = complaint.SellerId,
                Type = NotificationTypeEnum.ComplaintCreated,
                Title = "Khiếu nại đơn hàng mới",
                Message = $"Bạn có khiếu nại mới cho đơn hàng #{complaint.Order.OrderCode}. Vui lòng phản hồi trong vòng 2 ngày.",
                OrderId = complaint.OrderId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    ComplaintId = complaint.Id,
                    BuyerName = complaint.Buyer.FullName,
                    OrderCode = complaint.Order.OrderCode,
                    DeadlineHours = 48
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification);
            dto.RequiresSound = true;
            dto.RequiresAction = true;

            await _notificationService.SendToUserAsync(complaint.SellerId, dto);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(complaint.SellerId);
            await _notificationService.UpdateUnreadCountAsync(complaint.SellerId, unreadCount);
        }

        public async Task NotifySellerRespondedAsync(OrderComplaint complaint, CancellationToken cancellationToken = default)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = complaint.BuyerId,
                Type = NotificationTypeEnum.ComplaintUpdated,
                Title = "Người bán đã phản hồi khiếu nại",
                Message = $"Người bán đã phản hồi khiếu nại của bạn cho đơn hàng #{complaint.Order.OrderCode}. Đang chờ admin xử lý.",
                OrderId = complaint.OrderId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    ComplaintId = complaint.Id,
                    SellerName = complaint.Seller.FullName,
                    OrderCode = complaint.Order.OrderCode
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification);
            await _notificationService.SendToUserAsync(complaint.BuyerId, dto);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(complaint.BuyerId);
            await _notificationService.UpdateUnreadCountAsync(complaint.BuyerId, unreadCount);
        }

        public async Task NotifyComplaintApprovedAsync(OrderComplaint complaint, CancellationToken cancellationToken = default)
        {
            // Notify buyer
            var buyerNotification = new Domain.Entities.Notification
            {
                UserId = complaint.BuyerId,
                Type = NotificationTypeEnum.ComplaintResolved,
                Title = "Khiếu nại được chấp nhận",
                Message = $"Khiếu nại của bạn cho đơn hàng #{complaint.Order.OrderCode} đã được chấp nhận. Bạn sẽ nhận được hoàn tiền {complaint.ApprovedRefundAmount:N0} VNĐ.",
                OrderId = complaint.OrderId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    ComplaintId = complaint.Id,
                    RefundAmount = complaint.ApprovedRefundAmount,
                    OrderCode = complaint.Order.OrderCode
                })
            };

            await _notificationRepository.AddAsync(buyerNotification);

            // Notify seller
            var sellerNotification = new Domain.Entities.Notification
            {
                UserId = complaint.SellerId,
                Type = NotificationTypeEnum.ComplaintResolved,
                Title = "Khiếu nại được chấp nhận",
                Message = $"Khiếu nại cho đơn hàng #{complaint.Order.OrderCode} đã được chấp nhận. Số tiền hoàn lại: {complaint.ApprovedRefundAmount:N0} VNĐ.",
                OrderId = complaint.OrderId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    ComplaintId = complaint.Id,
                    RefundAmount = complaint.ApprovedRefundAmount,
                    OrderCode = complaint.Order.OrderCode
                })
            };

            await _notificationRepository.AddAsync(sellerNotification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send real-time notifications
            var buyerDto = MapToDto(buyerNotification);
            buyerDto.RequiresSound = true;
            await _notificationService.SendToUserAsync(complaint.BuyerId, buyerDto);

            var sellerDto = MapToDto(sellerNotification);
            sellerDto.RequiresSound = true;
            await _notificationService.SendToUserAsync(complaint.SellerId, sellerDto);

            // Update unread counts
            var buyerUnreadCount = await _notificationRepository.GetUnreadCountAsync(complaint.BuyerId);
            await _notificationService.UpdateUnreadCountAsync(complaint.BuyerId, buyerUnreadCount);

            var sellerUnreadCount = await _notificationRepository.GetUnreadCountAsync(complaint.SellerId);
            await _notificationService.UpdateUnreadCountAsync(complaint.SellerId, sellerUnreadCount);
        }

        public async Task NotifyComplaintRejectedAsync(OrderComplaint complaint, CancellationToken cancellationToken = default)
        {
            // Notify buyer
            var buyerNotification = new Domain.Entities.Notification
            {
                UserId = complaint.BuyerId,
                Type = NotificationTypeEnum.ComplaintResolved,
                Title = "Khiếu nại bị từ chối",
                Message = $"Khiếu nại của bạn cho đơn hàng #{complaint.Order.OrderCode} đã bị từ chối.",
                OrderId = complaint.OrderId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    ComplaintId = complaint.Id,
                    Reason = complaint.AdminDecisionReason,
                    OrderCode = complaint.Order.OrderCode
                })
            };

            await _notificationRepository.AddAsync(buyerNotification);

            // Notify seller
            var sellerNotification = new Domain.Entities.Notification
            {
                UserId = complaint.SellerId,
                Type = NotificationTypeEnum.ComplaintResolved,
                Title = "Khiếu nại bị từ chối",
                Message = $"Khiếu nại cho đơn hàng #{complaint.Order.OrderCode} đã bị từ chối.",
                OrderId = complaint.OrderId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    ComplaintId = complaint.Id,
                    Reason = complaint.AdminDecisionReason,
                    OrderCode = complaint.Order.OrderCode
                })
            };

            await _notificationRepository.AddAsync(sellerNotification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send real-time notifications
            await _notificationService.SendToUserAsync(complaint.BuyerId, MapToDto(buyerNotification));
            await _notificationService.SendToUserAsync(complaint.SellerId, MapToDto(sellerNotification));

            // Update unread counts
            var buyerUnreadCount = await _notificationRepository.GetUnreadCountAsync(complaint.BuyerId);
            await _notificationService.UpdateUnreadCountAsync(complaint.BuyerId, buyerUnreadCount);

            var sellerUnreadCount = await _notificationRepository.GetUnreadCountAsync(complaint.SellerId);
            await _notificationService.UpdateUnreadCountAsync(complaint.SellerId, sellerUnreadCount);
        }

        public async Task NotifyComplaintEscalatedAsync(OrderComplaint complaint, CancellationToken cancellationToken = default)
        {
            // Note: In production, you would send this to all admin users
            // For now, we'll create a notification that can be queried by admins
            // You could also use a broadcast mechanism or store admin user IDs in config

            // Notify buyer that complaint has been escalated
            var buyerNotification = new Domain.Entities.Notification
            {
                UserId = complaint.BuyerId,
                Type = NotificationTypeEnum.ComplaintUpdated,
                Title = "Khiếu nại đã chuyển đến admin",
                Message = $"Người bán không phản hồi khiếu nại của bạn cho đơn hàng #{complaint.Order.OrderCode} trong 2 ngày. Admin sẽ xem xét và xử lý.",
                OrderId = complaint.OrderId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    ComplaintId = complaint.Id,
                    OrderCode = complaint.Order.OrderCode,
                    Reason = "Seller did not respond within 48 hours"
                })
            };

            await _notificationRepository.AddAsync(buyerNotification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send real-time notification to buyer
            var buyerDto = MapToDto(buyerNotification);
            await _notificationService.SendToUserAsync(complaint.BuyerId, buyerDto);

            // Update unread count
            var buyerUnreadCount = await _notificationRepository.GetUnreadCountAsync(complaint.BuyerId);
            await _notificationService.UpdateUnreadCountAsync(complaint.BuyerId, buyerUnreadCount);

            // Notify admins about the escalated complaint
            await NotifyAdminPendingComplaintAsync(complaint, "auto-escalated", cancellationToken);
        }

        public async Task NotifyAdminPendingComplaintAsync(OrderComplaint complaint, string reason, CancellationToken cancellationToken = default)
        {
            // Send notification to admin group via SignalR
            var adminNotification = new NotificationDto
            {
                Id = Guid.NewGuid(),
                Title = "Khiếu nại cần xử lý",
                Message = reason == "auto-escalated" 
                    ? $"Khiếu nại cho đơn hàng #{complaint.Order.OrderCode} đã tự động chuyển đến admin (người bán không phản hồi trong 48h)."
                    : $"Có khiếu nại mới cần xử lý cho đơn hàng #{complaint.Order.OrderCode}. Người bán đã phản hồi.",
                Type = NotificationTypeEnum.ComplaintUpdated,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                OrderId = complaint.OrderId,
                RequiresSound = true,
                RequiresAction = true,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    ComplaintId = complaint.Id,
                    OrderCode = complaint.Order.OrderCode,
                    BuyerName = complaint.Buyer.FullName,
                    SellerName = complaint.Seller.FullName,
                    ShopName = complaint.Order.Shop.ShopName,
                    Reason = reason
                })
            };

            // Send to admin group (SignalR will handle broadcasting to all connected admins)
            await _notificationService.SendToGroupAsync("Admin", adminNotification);
        }

        private NotificationDto MapToDto(Domain.Entities.Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAtUtc,
                OrderId = notification.OrderId,
                ShopId = notification.ShopId,
                MetadataJson = notification.MetadataJson
            };
        }
    }
}
