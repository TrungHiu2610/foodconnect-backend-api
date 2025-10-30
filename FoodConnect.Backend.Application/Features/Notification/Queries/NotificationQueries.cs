using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Notification.Queries
{
    public class GetUserNotificationsQuery : IRequest<BaseResponse<List<NotificationDto>>>
    {
        public int Limit { get; set; } = 20;
        public int Offset { get; set; } = 0;
    }
    
    public class GetUnreadNotificationsQuery : IRequest<BaseResponse<List<NotificationDto>>>
    {
    }
    
    public class GetNotificationSummaryQuery : IRequest<BaseResponse<NotificationSummaryDto>>
    {
    }
}
