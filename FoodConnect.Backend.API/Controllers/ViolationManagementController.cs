using FoodConnect.Backend.Application.Features.Admin.Commands;
using FoodConnect.Backend.Application.Features.Admin.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ViolationManagementController : ApiBaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAccountViolationList(
            [FromQuery] string? role = null,
            [FromQuery] decimal? minScore = null,
            [FromQuery] decimal? maxScore = null,
            [FromQuery] bool? hasWarningBadge = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "Score",
            [FromQuery] string? sortOrder = "desc")
        {
            var query = new GetAccountViolationListQuery
            {
                Role = role,
                MinScore = minScore,
                MaxScore = maxScore,
                HasWarningBadge = hasWarningBadge,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await Mediator.Send(query);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAccountActivityDetail(
            [FromRoute] Guid userId,
            [FromQuery] int recentOrdersLimit = 20)
        {
            var query = new GetAccountActivityDetailQuery
            {
                UserId = userId,
                RecentOrdersLimit = recentOrdersLimit
            };

            var result = await Mediator.Send(query);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> LockAccount(
            [FromQuery] Guid userId,
            [FromQuery] string? reason = null)
        {
            var command = new ChangeUserStatusCommand
            {
                UserId = userId,
                NewStatus = Domain.Enums.UserStatusEnum.Locked,
                Reason = reason ?? "Account locked by admin due to violations"
            };

            var result = await Mediator.Send(command);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> UnlockAccount(
            [FromQuery] Guid userId,
            [FromQuery] string? reason = null)
        {
            var command = new ChangeUserStatusCommand
            {
                UserId = userId,
                NewStatus = Domain.Enums.UserStatusEnum.Active,
                Reason = reason ?? "Account unlocked by admin"
            };

            var result = await Mediator.Send(command);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> BanAccount(
            [FromQuery] Guid userId,
            [FromQuery] string? reason = null)
        {
            var command = new ChangeUserStatusCommand
            {
                UserId = userId,
                NewStatus = Domain.Enums.UserStatusEnum.Banned,
                Reason = reason ?? "Account banned by admin due to severe violations"
            };

            var result = await Mediator.Send(command);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }
    }
}
