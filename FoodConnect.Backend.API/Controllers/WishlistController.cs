using FoodConnect.Backend.Application.Features.Wishlist.Commands;
using FoodConnect.Backend.Application.Features.Wishlist.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class WishlistController : ApiBaseController
    {
        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpDelete("{wishlistId}")]
        public async Task<IActionResult> RemoveFromWishlist([FromRoute] Guid wishlistId)
        {
            var command = new RemoveFromWishlistCommand { WishlistId = wishlistId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateNotification([FromBody] UpdateWishlistNotificationCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetMyWishlist([FromQuery] int? type)
        {
            var query = new GetMyWishlistQuery { Type = type };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
