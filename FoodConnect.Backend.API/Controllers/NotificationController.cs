using FoodConnect.Backend.API.Controllers;
using FoodConnect.Backend.Application.Features.Notification.Commands;
using FoodConnect.Backend.Application.Features.Notification.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController : ApiBaseController
    {
        /// <summary>
        /// Get user's notifications with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int limit = 20, [FromQuery] int offset = 0)
        {
            var query = new GetUserNotificationsQuery { Limit = limit, Offset = offset };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Get unread notifications only
        /// </summary>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var query = new GetUnreadNotificationsQuery();
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Get notification summary (unread count + recent)
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var query = new GetNotificationSummaryQuery();
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Mark single notification as read
        /// </summary>
        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var command = new MarkNotificationAsReadCommand { NotificationId = notificationId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Mark multiple notifications as read
        /// </summary>
        [HttpPut("read-multiple")]
        public async Task<IActionResult> MarkMultipleAsRead([FromBody] MarkMultipleNotificationsAsReadCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var command = new MarkAllNotificationsAsReadCommand();
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
