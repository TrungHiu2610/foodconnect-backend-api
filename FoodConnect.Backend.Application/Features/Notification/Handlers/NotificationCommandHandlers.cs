using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Commands;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Notification.Handlers
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, BaseResponse<NotificationActionResult>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public MarkNotificationAsReadCommandHandler(
            INotificationRepository notificationRepository,
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<NotificationActionResult>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<NotificationActionResult>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            await _notificationRepository.MarkAsReadAsync(request.NotificationId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update unread count via SignalR
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(_currentUserService.UserId.Value);
            await _notificationService.UpdateUnreadCountAsync(_currentUserService.UserId.Value, unreadCount);

            var actionResult = new NotificationActionResult
            {
                Success = true,
                AffectedCount = 1,
                Message = "Notification marked as read"
            };

            return result.BuildSuccess(actionResult, "Notification marked as read");
        }
    }

    public class MarkMultipleNotificationsAsReadCommandHandler : IRequestHandler<MarkMultipleNotificationsAsReadCommand, BaseResponse<NotificationActionResult>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public MarkMultipleNotificationsAsReadCommandHandler(
            INotificationRepository notificationRepository,
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<NotificationActionResult>> Handle(MarkMultipleNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<NotificationActionResult>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            await _notificationRepository.MarkMultipleAsReadAsync(request.NotificationIds);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update unread count via SignalR
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(_currentUserService.UserId.Value);
            await _notificationService.UpdateUnreadCountAsync(_currentUserService.UserId.Value, unreadCount);

            var actionResult = new NotificationActionResult
            {
                Success = true,
                AffectedCount = request.NotificationIds.Count,
                Message = $"{request.NotificationIds.Count} notifications marked as read"
            };

            return result.BuildSuccess(actionResult, $"{request.NotificationIds.Count} notifications marked as read");
        }
    }

    public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, BaseResponse<NotificationActionResult>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public MarkAllNotificationsAsReadCommandHandler(
            INotificationRepository notificationRepository,
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<NotificationActionResult>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<NotificationActionResult>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            // Get unread count before marking
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(_currentUserService.UserId.Value);
            
            await _notificationRepository.MarkAllAsReadAsync(_currentUserService.UserId.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update unread count to 0
            await _notificationService.UpdateUnreadCountAsync(_currentUserService.UserId.Value, 0);

            var actionResult = new NotificationActionResult
            {
                Success = true,
                AffectedCount = unreadCount,
                Message = "All notifications marked as read"
            };

            return result.BuildSuccess(actionResult, "All notifications marked as read");
        }
    }
}
