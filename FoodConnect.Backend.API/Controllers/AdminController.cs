using FoodConnect.Backend.Application.Features.Admin.Commands;
using FoodConnect.Backend.Application.Features.Admin.Queries;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ApiBaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetUserList(
            [FromQuery] string? searchTerm,
            [FromQuery] UserStatusEnum? status,
            [FromQuery] RoleEnum? role,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "CreatedAtUtc",
            [FromQuery] bool isDescending = true)
        {
            var query = new GetUserListQuery
            {
                SearchTerm = searchTerm,
                Status = status,
                Role = role,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                IsDescending = isDescending
            };

            var result = await Mediator.Send(query);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserDetail([FromRoute] Guid userId)
        {
            var query = new GetUserDetailQuery { UserId = userId };
            var result = await Mediator.Send(query);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> ChangeUserStatus([FromBody] ChangeUserStatusCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetFlaggedReviews(
            [FromQuery] ReviewStatusEnum? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetFlaggedReviewsQuery
            {
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await Mediator.Send(query);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }
    }
}
