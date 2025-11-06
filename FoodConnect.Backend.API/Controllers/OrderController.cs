using FoodConnect.Backend.Application.Features.Order.Commands;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Queries;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class OrderController : ApiBaseController
    {
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetail([FromQuery] Guid orderId)
        {
            var query = new GetOrderDetailQuery { OrderId = orderId };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders([FromQuery] OrderStatusEnum? status = null)
        {
            var query = new GetOrdersByBuyerQuery { Status = status };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> CancelOrder([FromQuery] Guid orderId, [FromBody] CancelOrderDto dto)
        {
            var command = new CancelOrderCommand 
            { 
                OrderId = orderId,
                CancelReason = dto.CancelReason 
            };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> ConfirmOrderReceived([FromQuery] Guid orderId)
        {
            var command = new ConfirmOrderReceivedCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetShopOrders([FromQuery] Guid shopId, [FromQuery] OrderStatusEnum? status = null)
        {
            var query = new GetOrdersByShopQuery 
            { 
                ShopId = shopId,
                Status = status 
            };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> AcceptOrder([FromQuery] Guid orderId)
        {
            var command = new AcceptOrderCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> RejectOrder([FromQuery] Guid orderId, [FromBody] RejectOrderDto dto)
        {
            var command = new RejectOrderCommand 
            { 
                OrderId = orderId,
                RejectionReason = dto.RejectionReason 
            };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> MarkAsPrepared([FromQuery] Guid orderId)
        {
            var command = new MarkAsPreparedCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> MarkAsDelivered([FromQuery] Guid orderId)
        {
            var command = new MarkAsDeliveredCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
