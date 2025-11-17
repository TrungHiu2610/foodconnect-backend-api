using FoodConnect.Backend.API.Controllers;
using FoodConnect.Backend.Application.Features.Notification.Commands;
using FoodConnect.Backend.Application.Features.Notification.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ApiBaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int limit = 20, [FromQuery] int offset = 0)
        {
            var query = new GetUserNotificationsQuery { Limit = limit, Offset = offset };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var query = new GetUnreadNotificationsQuery();
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetSummary()
        {
            var query = new GetNotificationSummaryQuery();
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> MarkAsRead([FromQuery] Guid notificationId)
        {
            var command = new MarkNotificationAsReadCommand { NotificationId = notificationId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> MarkMultipleAsRead([FromBody] MarkMultipleNotificationsAsReadCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var command = new MarkAllNotificationsAsReadCommand();
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
