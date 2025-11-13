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
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            var query = new GetCartQuery { SessionId = sessionId };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartCommand command)
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            command.SessionId = sessionId;
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemCommand command)
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            command.SessionId = sessionId;
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveCartItem([FromBody] RemoveCartItemCommand command)
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            command.SessionId = sessionId;
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            var command = new ClearCartCommand { SessionId = sessionId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Get cart item count for header badge (lightweight)
        /// Header: X-Session-Id (for guest users)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            var query = new GetCartCountQuery { SessionId = sessionId };
            var result = await Mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> ValidateCart([FromBody] ValidateCartQuery query)
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            query.SessionId = sessionId;
            var result = await Mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get checkout preview - shows how cart will be split into orders with shipping fees
        /// Used for Checkout Page (not Cart Page)
        /// Header: X-Session-Id (for guest users)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetCheckoutPreview([FromBody] GetCheckoutPreviewQuery query)
        {
            var sessionId = Request.Headers["X-Session-Id"].ToString();
            query.SessionId = sessionId;
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
