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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllMyShops()
        {
            var query = new GetAllMyShopsQuery();
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

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetShopDetail(Guid id, [FromQuery]GetShopDetailQuery query)
        {
            query.ShopId = id;
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> GetListShopsForBuyer([FromBody] GetListShopsForBuyerQuery query)
        {
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpGet]
        public async Task<IActionResult> GetFeaturedShops([FromQuery] int limit = 10, [FromQuery] double? userLatitude = null, [FromQuery] double? userLongitude = null)
        {
            var query = new GetFeaturedShopsQuery
            {
                Limit = limit,
                UserLatitude = userLatitude,
                UserLongitude = userLongitude
            };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpGet("{shopId}")]
        public async Task<IActionResult> GetShopDetailForBuyer([FromRoute] Guid shopId, [FromQuery] double? userLatitude = null, [FromQuery] double? userLongitude = null)
        {
            var query = new GetShopDetailForBuyerQuery
            {
                ShopId = shopId,
                UserLatitude = userLatitude,
                UserLongitude = userLongitude
            };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> FilterByLocation(
            [FromQuery] int? deliveryType, 
            [FromQuery] double? buyerLatitude, 
            [FromQuery] double? buyerLongitude)
        {
            var query = new FilterShopsByLocationQuery
            {
                DeliveryType = deliveryType.HasValue ? (DeliveryTypeEnum)deliveryType.Value : null,
                BuyerLatitude = buyerLatitude,
                BuyerLongitude = buyerLongitude
            };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
