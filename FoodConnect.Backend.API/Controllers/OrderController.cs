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
        public async Task<IActionResult> CalculateShippingFee([FromQuery] CalculateShippingFeeQuery query)
        {
            var result = await Mediator.Send(query);
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
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] OrderStatusEnum? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetOrdersByBuyerQuery 
            { 
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
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
        public async Task<IActionResult> GetShopOrders(
            [FromQuery] Guid shopId, 
            [FromQuery] OrderStatusEnum? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetOrdersByShopQuery 
            { 
                ShopId = shopId,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
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

        [HttpPut]
        public async Task<IActionResult> MarkOrderReady([FromQuery] Guid orderId)
        {
            var command = new MarkOrderReadyCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> StartSelfDelivery([FromQuery] Guid orderId)
        {
            var command = new StartSelfDeliveryCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> RequestShipper([FromQuery] Guid orderId)
        {
            var command = new RequestShipperCommand { OrderId = orderId };
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> ConfirmDeliveryWithProof([FromQuery] Guid orderId, [FromForm] ConfirmDeliveryWithProofCommand command)
        {
            command.OrderId = orderId;
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
