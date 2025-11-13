using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Queries;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Notification.Handlers
{
    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, BaseResponse<List<NotificationDto>>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetUserNotificationsQueryHandler(
            INotificationRepository notificationRepository,
            ICurrentUserService currentUserService)
        {
            _notificationRepository = notificationRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<List<NotificationDto>>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<NotificationDto>>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var notifications = await _notificationRepository.GetUserNotificationsAsync(
                _currentUserService.UserId.Value,
                request.Limit,
                request.Offset
            );

            var dtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAtUtc,
                ReadAt = n.ReadAt,
                OrderId = n.OrderId,
                OrderCode = n.Order?.OrderCode,
                ShopId = n.ShopId,
                ShopName = n.Shop?.ShopName,
                MetadataJson = n.MetadataJson
            }).ToList();

            return result.BuildSuccess(dtos, "Notifications retrieved successfully");
        }
    }

    public class GetUnreadNotificationsQueryHandler : IRequestHandler<GetUnreadNotificationsQuery, BaseResponse<List<NotificationDto>>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetUnreadNotificationsQueryHandler(
            INotificationRepository notificationRepository,
            ICurrentUserService currentUserService)
        {
            _notificationRepository = notificationRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<List<NotificationDto>>> Handle(GetUnreadNotificationsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<NotificationDto>>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var notifications = await _notificationRepository.GetUnreadNotificationsAsync(
                _currentUserService.UserId.Value
            );

            var dtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAtUtc,
                ReadAt = n.ReadAt,
                OrderId = n.OrderId,
                OrderCode = n.Order?.OrderCode,
                ShopId = n.ShopId,
                ShopName = n.Shop?.ShopName,
                MetadataJson = n.MetadataJson
            }).ToList();

            return result.BuildSuccess(dtos, "Unread notifications retrieved successfully");
        }
    }

    public class GetNotificationSummaryQueryHandler : IRequestHandler<GetNotificationSummaryQuery, BaseResponse<NotificationSummaryDto>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetNotificationSummaryQueryHandler(
            INotificationRepository notificationRepository,
            ICurrentUserService currentUserService)
        {
            _notificationRepository = notificationRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<NotificationSummaryDto>> Handle(GetNotificationSummaryQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<NotificationSummaryDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var userId = _currentUserService.UserId.Value;
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);
            var recent = await _notificationRepository.GetUserNotificationsAsync(userId, 5, 0);

            var summary = new NotificationSummaryDto
            {
                UnreadCount = unreadCount,
                TotalCount = recent.Count,
                Recent = recent.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAtUtc,
                    ReadAt = n.ReadAt,
                    OrderId = n.OrderId,
                    OrderCode = n.Order?.OrderCode,
                    ShopId = n.ShopId,
                    ShopName = n.Shop?.ShopName,
                    MetadataJson = n.MetadataJson
                }).ToList()
            };

            return result.BuildSuccess(summary, "Notification summary retrieved successfully");
        }
    }
}
