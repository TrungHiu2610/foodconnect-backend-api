using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using System.Text.Json;

namespace FoodConnect.Backend.Application.Features.Promotion.Services
{
    public class PromotionNotificationService
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PromotionNotificationService(
            INotificationService notificationService,
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task NotifyPromotionCreatedAsync(Domain.Entities.Promotion promotion, CancellationToken cancellationToken = default)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = promotion.Shop!.UserId,
                Type = NotificationTypeEnum.SystemAnnouncement,
                Title = "Khuyến mãi mới đã được tạo",
                Message = $"Khuyến mãi \"{promotion.PromotionName}\" đã được tạo thành công và đang ở trạng thái {GetStatusText(promotion.Status)}",
                ShopId = promotion.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    PromotionId = promotion.Id,
                    PromotionName = promotion.PromotionName,
                    DiscountValue = promotion.DiscountValue,
                    PromotionType = promotion.PromotionType.ToString(),
                    StartDate = promotion.StartDate,
                    EndDate = promotion.EndDate,
                    Status = promotion.Status.ToString()
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification, promotion);
            await _notificationService.SendToUserAsync(promotion.Shop.UserId, dto);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(promotion.Shop.UserId);
            await _notificationService.UpdateUnreadCountAsync(promotion.Shop.UserId, unreadCount);
        }

        public async Task NotifyPromotionActivatedAsync(Domain.Entities.Promotion promotion, CancellationToken cancellationToken = default)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = promotion.Shop!.UserId,
                Type = NotificationTypeEnum.SystemAnnouncement,
                Title = "Khuyến mãi đã được kích hoạt",
                Message = $"Khuyến mãi \"{promotion.PromotionName}\" đã được kích hoạt và khách hàng có thể sử dụng",
                ShopId = promotion.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    PromotionId = promotion.Id,
                    PromotionName = promotion.PromotionName,
                    DiscountValue = promotion.DiscountValue,
                    PromotionType = promotion.PromotionType.ToString(),
                    EndDate = promotion.EndDate
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification, promotion);
            await _notificationService.SendToUserAsync(promotion.Shop.UserId, dto);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(promotion.Shop.UserId);
            await _notificationService.UpdateUnreadCountAsync(promotion.Shop.UserId, unreadCount);
        }
        public async Task NotifyPromotionDeactivatedAsync(Domain.Entities.Promotion promotion, CancellationToken cancellationToken = default)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = promotion.Shop!.UserId,
                Type = NotificationTypeEnum.SystemAnnouncement,
                Title = "⏸️ Khuyến mãi đã bị tạm dừng",
                Message = $"Khuyến mãi \"{promotion.PromotionName}\" đã bị tạm dừng và không còn khả dụng cho khách hàng",
                ShopId = promotion.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    PromotionId = promotion.Id,
                    PromotionName = promotion.PromotionName
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification, promotion);
            await _notificationService.SendToUserAsync(promotion.Shop.UserId, dto);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(promotion.Shop.UserId);
            await _notificationService.UpdateUnreadCountAsync(promotion.Shop.UserId, unreadCount);
        }
        public async Task NotifyPromotionExpiringAsync(Domain.Entities.Promotion promotion, CancellationToken cancellationToken = default)
        {
            var hoursRemaining = (promotion.EndDate - DateTime.UtcNow).TotalHours;

            var notification = new Domain.Entities.Notification
            {
                UserId = promotion.Shop!.UserId,
                Type = NotificationTypeEnum.SystemAnnouncement,
                Title = "Khuyến mãi sắp hết hạn",
                Message = $"Khuyến mãi \"{promotion.PromotionName}\" sẽ hết hạn trong {Math.Round(hoursRemaining)} giờ nữa",
                ShopId = promotion.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    PromotionId = promotion.Id,
                    PromotionName = promotion.PromotionName,
                    EndDate = promotion.EndDate,
                    HoursRemaining = Math.Round(hoursRemaining, 1)
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification, promotion);
            await _notificationService.SendToUserAsync(promotion.Shop.UserId, dto);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(promotion.Shop.UserId);
            await _notificationService.UpdateUnreadCountAsync(promotion.Shop.UserId, unreadCount);
        }

        public async Task NotifyPromotionExpiredAsync(Domain.Entities.Promotion promotion, CancellationToken cancellationToken = default)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = promotion.Shop!.UserId,
                Type = NotificationTypeEnum.SystemAnnouncement,
                Title = "Khuyến mãi đã hết hạn",
                Message = $"Khuyến mãi \"{promotion.PromotionName}\" đã hết hạn và không còn khả dụng",
                ShopId = promotion.ShopId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    PromotionId = promotion.Id,
                    PromotionName = promotion.PromotionName,
                    EndDate = promotion.EndDate,
                    TotalUsedCount = promotion.TotalUsedCount
                })
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(notification, promotion);
            await _notificationService.SendToUserAsync(promotion.Shop.UserId, dto);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(promotion.Shop.UserId);
            await _notificationService.UpdateUnreadCountAsync(promotion.Shop.UserId, unreadCount);
        }

        private NotificationDto MapToDto(Domain.Entities.Notification notification, Domain.Entities.Promotion promotion)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                ShopId = notification.ShopId,
                ShopName = promotion.Shop?.ShopName,
                MetadataJson = notification.MetadataJson,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAtUtc
            };
        }

        private string GetStatusText(PromotionStatusEnum status)
        {
            return status switch
            {
                PromotionStatusEnum.Draft => "Bản nháp",
                PromotionStatusEnum.PendingApproval => "Chờ duyệt",
                PromotionStatusEnum.Approved => "Đã duyệt",
                PromotionStatusEnum.Active => "Đang hoạt động",
                PromotionStatusEnum.Rejected => "Bị từ chối",
                PromotionStatusEnum.Expired => "Đã hết hạn",
                PromotionStatusEnum.Cancelled => "Đã hủy",
                _ => status.ToString()
            };
        }
    }
}
