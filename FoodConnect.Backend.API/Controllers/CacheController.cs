using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Cache.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CacheController : ApiBaseController
    {
        private readonly IMediator _mediator;

        public CacheController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("invalidate")]
        public async Task<ActionResult<BaseResponse<string>>> InvalidateCache([FromBody] InvalidateCacheCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("invalidate-all")]
        public async Task<ActionResult<BaseResponse<string>>> InvalidateAllCache()
        {
            var command = new InvalidateCacheCommand { InvalidateAll = true };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        
        [HttpPost("invalidate-seller/{shopId}")]
        public async Task<ActionResult<BaseResponse<string>>> InvalidateSellerCache(Guid shopId)
        {
            var command = new InvalidateCacheCommand { ShopId = shopId };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("invalidate-buyer/{buyerId}")]
        public async Task<ActionResult<BaseResponse<string>>> InvalidateBuyerCache(Guid buyerId)
        {
            var command = new InvalidateCacheCommand { BuyerId = buyerId };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("invalidate-admin-stats")]
        public async Task<ActionResult<BaseResponse<string>>> InvalidateAdminStats()
        {
            var command = new InvalidateCacheCommand { CachePattern = "admin:statistics:*" };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
