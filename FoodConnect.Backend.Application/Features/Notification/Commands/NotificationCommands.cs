using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Notification.Commands
{
    public class MarkNotificationAsReadCommand : IRequest<BaseResponse<NotificationActionResult>>
    {
        public Guid NotificationId { get; set; }
    }

    public class MarkMultipleNotificationsAsReadCommand : IRequest<BaseResponse<NotificationActionResult>>
    {
        public List<Guid> NotificationIds { get; set; } = new List<Guid>();
    }

    public class MarkAllNotificationsAsReadCommand : IRequest<BaseResponse<NotificationActionResult>>
    {
    }
}
