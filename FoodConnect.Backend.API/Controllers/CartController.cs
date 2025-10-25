using FoodConnect.Backend.Application.Features.Cart.Commands;
using FoodConnect.Backend.Application.Features.Cart.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CartController : ApiBaseController
    {
        /// <summary>
        /// Get cart for current user or guest (with session ID in header)
        /// Header: X-Session-Id
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            var query = new GetCartQuery { SessionId = sessionId };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Add product to cart
        /// Header: X-Session-Id (for guest users)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartCommand command)
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            command.SessionId = sessionId;
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Update cart item quantity
        /// Header: X-Session-Id (for guest users)
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemCommand command)
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            command.SessionId = sessionId;
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Remove item from cart
        /// Header: X-Session-Id (for guest users)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> RemoveCartItem([FromBody] RemoveCartItemCommand command)
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            command.SessionId = sessionId;
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Clear all items from cart
        /// Header: X-Session-Id (for guest users)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            var command = new ClearCartCommand { SessionId = sessionId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
