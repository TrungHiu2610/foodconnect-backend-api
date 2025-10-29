using FoodConnect.Backend.Application.Features.Order.Commands;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Queries;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController : ApiBaseController
    {
        #region Buyer Endpoints

        /// <summary>
        /// Create orders from cart items (buyer)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Get order detail by ID (buyer/seller)
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetail(Guid orderId)
        {
            var query = new GetOrderDetailQuery { OrderId = orderId };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Get all orders for current buyer
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders([FromQuery] OrderStatusEnum? status = null)
        {
            var query = new GetOrdersByBuyerQuery { Status = status };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Cancel order (buyer, only pending orders)
        /// </summary>
        [HttpPut("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid orderId, [FromBody] CancelOrderDto dto)
        {
            var command = new CancelOrderCommand 
            { 
                OrderId = orderId,
                CancelReason = dto.CancelReason 
            };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Confirm order received (buyer, only delivered orders)
        /// </summary>
        [HttpPut("{orderId}/confirm-received")]
        public async Task<IActionResult> ConfirmOrderReceived(Guid orderId)
        {
            var command = new ConfirmOrderReceivedCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        #endregion

        #region Seller Endpoints

        /// <summary>
        /// Get all orders for a shop (seller)
        /// </summary>
        [HttpGet("shop/{shopId}")]
        public async Task<IActionResult> GetShopOrders(Guid shopId, [FromQuery] OrderStatusEnum? status = null)
        {
            var query = new GetOrdersByShopQuery 
            { 
                ShopId = shopId,
                Status = status 
            };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Accept order (seller, only pending orders)
        /// </summary>
        [HttpPut("{orderId}/accept")]
        public async Task<IActionResult> AcceptOrder(Guid orderId)
        {
            var command = new AcceptOrderCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Reject order (seller, only pending orders)
        /// </summary>
        [HttpPut("{orderId}/reject")]
        public async Task<IActionResult> RejectOrder(Guid orderId, [FromBody] RejectOrderDto dto)
        {
            var command = new RejectOrderCommand 
            { 
                OrderId = orderId,
                RejectionReason = dto.RejectionReason 
            };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Mark order as prepared (seller, only preparing orders)
        /// </summary>
        [HttpPut("{orderId}/mark-prepared")]
        public async Task<IActionResult> MarkAsPrepared(Guid orderId)
        {
            var command = new MarkAsPreparedCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        /// <summary>
        /// Mark order as delivered (seller/simulated shipper, only out-for-delivery orders)
        /// </summary>
        [HttpPut("{orderId}/mark-delivered")]
        public async Task<IActionResult> MarkAsDelivered(Guid orderId)
        {
            var command = new MarkAsDeliveredCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        #endregion
    }
}
