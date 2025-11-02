using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using System.Text.Json;
using OrderEntity = FoodConnect.Backend.Domain.Entities.Order;

namespace FoodConnect.Backend.Application.Features.Notification.Services
{
    public class OrderNotificationService
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderNotificationService(
            INotificationService notificationService,
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task NotifyNewOrderAsync(OrderEntity order, CancellationToken cancellationToken = default)
        {
            // Create notification for seller
            var notification = new Domain.Entities.Notification
            {
                UserId = order.Shop!.UserId,
                Type = NotificationTypeEnum.NewOrder,
                Title = "Đơn hàng mới",
                Message = $"Bạn có đơn hàng mới #{order.OrderCode} từ {order.Buyer?.FullName}. Tổng tiền: {order.Total:N0} VNĐ",
                OrderId = order.Id,
                ShopId = order.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    BuyerName = order.Buyer?.FullName,
                    Total = order.Total,
                    ItemCount = order.OrderItems?.Count ?? 0
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send real-time notification with sound alert
            var dto = MapToDto(notification, order);
            
            dto.RequiresSound = true;
            dto.SoundRepeatIntervalSeconds = 15; 
            dto.RequiresAction = true; 
            
            await _notificationService.SendNewOrderAlertAsync(order.Shop.UserId, dto);

            // Update unread count
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(order.Shop.UserId);
            await _notificationService.UpdateUnreadCountAsync(order.Shop.UserId, unreadCount);
        }

        public async Task NotifyOrderAcceptedAsync(OrderEntity order, CancellationToken cancellationToken = default)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = order.BuyerId,
                Type = NotificationTypeEnum.OrderAccepted,
                Title = "Đơn hàng đã được chấp nhận",
                Message = $"Shop {order.Shop?.ShopName} đã chấp nhận đơn hàng #{order.OrderCode} của bạn",
                OrderId = order.Id,
                ShopId = order.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    ShopName = order.Shop?.ShopName,
                    AcceptedAt = order.AcceptedAt
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification, order);
            await _notificationService.SendToUserAsync(order.BuyerId, dto);
            await _notificationService.SendOrderStatusUpdateAsync(order.BuyerId, order.Id, "Preparing", "Đơn hàng đang được chuẩn bị");

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(order.BuyerId);
            await _notificationService.UpdateUnreadCountAsync(order.BuyerId, unreadCount);
        }

        public async Task NotifyOrderRejectedAsync(OrderEntity order, CancellationToken cancellationToken = default)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = order.BuyerId,
                Type = NotificationTypeEnum.OrderRejected,
                Title = "Đơn hàng bị từ chối",
                Message = $"Shop {order.Shop?.ShopName} đã từ chối đơn hàng #{order.OrderCode}. Lý do: {order.RejectionReason}",
                OrderId = order.Id,
                ShopId = order.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    ShopName = order.Shop?.ShopName,
                    RejectionReason = order.RejectionReason,
                    CancelledAt = order.CancelledAt
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification, order);
            await _notificationService.SendToUserAsync(order.BuyerId, dto);
            await _notificationService.SendOrderStatusUpdateAsync(order.BuyerId, order.Id, "Rejected", order.RejectionReason ?? "");

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(order.BuyerId);
            await _notificationService.UpdateUnreadCountAsync(order.BuyerId, unreadCount);
        }

        public async Task NotifyOrderPreparingAsync(OrderEntity order, CancellationToken cancellationToken = default)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = order.BuyerId,
                Type = NotificationTypeEnum.OrderPreparing,
                Title = "Đơn hàng đang chuẩn bị",
                Message = $"Shop {order.Shop?.ShopName} đang chuẩn bị đơn hàng #{order.OrderCode}",
                OrderId = order.Id,
                ShopId = order.ShopId
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification, order);
            await _notificationService.SendToUserAsync(order.BuyerId, dto);
            await _notificationService.SendOrderStatusUpdateAsync(order.BuyerId, order.Id, "OutForDelivery", "Đơn hàng đang được giao");

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(order.BuyerId);
            await _notificationService.UpdateUnreadCountAsync(order.BuyerId, unreadCount);
        }

        public async Task NotifyOrderDeliveredAsync(OrderEntity order, CancellationToken cancellationToken = default)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = order.BuyerId,
                Type = NotificationTypeEnum.OrderDelivered,
                Title = "Đơn hàng đã được giao",
                Message = $"Đơn hàng #{order.OrderCode} đã được giao. Vui lòng xác nhận đã nhận hàng!",
                OrderId = order.Id,
                ShopId = order.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    DeliveredAt = order.DeliveredAt
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification, order);
            await _notificationService.SendToUserAsync(order.BuyerId, dto);
            await _notificationService.SendOrderStatusUpdateAsync(order.BuyerId, order.Id, "Delivered", "Vui lòng xác nhận đã nhận hàng");

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(order.BuyerId);
            await _notificationService.UpdateUnreadCountAsync(order.BuyerId, unreadCount);
        }

        public async Task NotifyOrderCompletedAsync(OrderEntity order, CancellationToken cancellationToken = default)
        {
            // Notify seller
            var sellerNotification = new Domain.Entities.Notification
            {
                UserId = order.Shop!.UserId,
                Type = NotificationTypeEnum.OrderCompleted,
                Title = "Đơn hàng hoàn thành",
                Message = $"Khách hàng đã xác nhận nhận đơn hàng #{order.OrderCode}",
                OrderId = order.Id,
                ShopId = order.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    CompletedAt = order.CompletedAt,
                    BuyerName = order.Buyer?.FullName
                })
            };

            await _notificationRepository.AddAsync(sellerNotification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var sellerDto = MapToDto(sellerNotification, order);
            await _notificationService.SendToUserAsync(order.Shop.UserId, sellerDto);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(order.Shop.UserId);
            await _notificationService.UpdateUnreadCountAsync(order.Shop.UserId, unreadCount);
        }

        public async Task NotifyOrderCancelledAsync(OrderEntity order, bool isBuyerCancelled, CancellationToken cancellationToken = default)
        {
            Guid recipientUserId;
            string title;
            string message;

            if (isBuyerCancelled)
            {
                // Notify seller
                recipientUserId = order.Shop!.UserId;
                title = "Đơn hàng bị hủy";
                message = $"Khách hàng {order.Buyer?.FullName} đã hủy đơn hàng #{order.OrderCode}. Lý do: {order.CancelReason ?? "Không có"}";
            }
            else
            {
                // Notify buyer (if seller cancels - currently only buyer can cancel)
                recipientUserId = order.BuyerId;
                title = "Đơn hàng bị hủy";
                message = $"Đơn hàng #{order.OrderCode} đã bị hủy. Lý do: {order.CancelReason ?? "Không có"}";
            }

            var notification = new Domain.Entities.Notification
            {
                UserId = recipientUserId,
                Type = NotificationTypeEnum.OrderCancelled,
                Title = title,
                Message = message,
                OrderId = order.Id,
                ShopId = order.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    CancelReason = order.CancelReason,
                    CancelledAt = order.CancelledAt,
                    IsBuyerCancelled = isBuyerCancelled
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification, order);
            await _notificationService.SendToUserAsync(recipientUserId, dto);
            await _notificationService.SendOrderStatusUpdateAsync(recipientUserId, order.Id, "Cancelled", message);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(recipientUserId);
            await _notificationService.UpdateUnreadCountAsync(recipientUserId, unreadCount);
        }

        private NotificationDto MapToDto(Domain.Entities.Notification notification, OrderEntity order)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAtUtc,
                ReadAt = notification.ReadAt,
                OrderId = notification.OrderId,
                OrderCode = order.OrderCode,
                ShopId = notification.ShopId,
                ShopName = order.Shop?.ShopName,
                MetadataJson = notification.MetadataJson
            };
        }
    }
}
