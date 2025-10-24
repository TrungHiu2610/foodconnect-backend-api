using FoodConnect.Backend.Application.Features.ShopRegistrations.Commands;
using FoodConnect.Backend.Application.Features.ShopRegistrations.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ShopRegistrationController : ApiBaseController
    {
        /// <summary>
        /// Create a new shop registration (Buyer role)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> CreateShopRegistration([FromForm] CreateShopRegistrationCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Get my shop registration (current user)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyShopRegistration()
        {
            var query = new GetMyShopRegistrationQuery();
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Get shop registration detail by ID (Admin only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetShopRegistrationDetail(Guid id)
        {
            var query = new GetShopRegistrationDetailQuery { Id = id };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Get list of shop registrations with pagination and filter (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetListShopRegistrations([FromBody] GetListShopRegistrationsQuery query)
        {
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Approve a shop registration (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveShopRegistration([FromBody] ApproveShopRegistrationCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Reject a shop registration (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectShopRegistration([FromBody] RejectShopRegistrationCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Cancel a shop registration (User own registration)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CancelShopRegistration([FromBody] CancelShopRegistrationCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
