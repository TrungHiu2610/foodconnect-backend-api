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
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ShopController : ApiBaseController
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateShop([FromForm] CreateShopCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateShop([FromForm] UpdateShopCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitShop([FromBody] SubmitShopCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveShop([FromBody] ApproveShopCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectShop([FromBody] RejectShopCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyShop()
        {
            var query = new GetMyShopQuery();
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetListShops([FromBody] GetListShopsQuery query)
        {
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetShopDetail([FromRoute] Guid shopId)
        {
            var query = new GetShopDetailQuery { ShopId = shopId };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
