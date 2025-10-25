using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Features.Shop.Commands;
using FoodConnect.Backend.Application.Features.Shop.Queries;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    public class ShopController : ApiBaseController
    {
        private readonly IMediator _mediator;

        public ShopController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create new shop (Draft status) - User
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateShop([FromForm] CreateShopCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Submit shop for approval (Draft -> PendingApproval) - User
        /// </summary>
        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> SubmitShop([FromBody] SubmitShopCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Approve shop (PendingApproval -> Active) - Admin
        /// </summary>
        [HttpPost("approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveShop([FromBody] ApproveShopCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Reject shop (PendingApproval -> Rejected) - Admin
        /// </summary>
        [HttpPost("reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectShop([FromBody] RejectShopCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get current user's shop - User
        /// </summary>
        [HttpGet("my-shop")]
        [Authorize]
        public async Task<IActionResult> GetMyShop()
        {
            var query = new GetMyShopQuery();
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get list of shops with pagination and filtering - Admin
        /// </summary>
        [HttpPost("list")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetListShops([FromBody] GetListShopsQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get shop detail by ID - Admin
        /// </summary>
        [HttpGet("{shopId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetShopDetail([FromRoute] Guid shopId)
        {
            var query = new GetShopDetailQuery { ShopId = shopId };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }
    }
}
