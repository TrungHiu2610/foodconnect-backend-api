using FoodConnect.Backend.Application.Features.Promotion.Commands;
using FoodConnect.Backend.Application.Features.Promotion.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PromotionController : ApiBaseController
    {
        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreatePromotion([FromForm] CreatePromotionCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdatePromotion([FromForm] UpdatePromotionCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> SubmitPromotionForApproval([FromBody] SubmitPromotionForApprovalCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CancelPromotion([FromBody] CancelPromotionCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> GetMyPromotions([FromQuery] int? status)
        {
            var query = new GetMyPromotionsQuery { Status = status };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetPromotionDetail(Guid id, [FromQuery] GetPromotionDetailQuery query)
        {
            query.PromotionId = id;
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingPromotions([FromQuery] Guid? shopId)
        {
            var query = new GetPendingPromotionsQuery { ShopId = shopId };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPromotions([FromBody] GetAllPromotionsQuery query)
        {
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApprovePromotion([FromBody] ApprovePromotionCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectPromotion([FromBody] RejectPromotionCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet("{shopId}")]
        [Authorize]
        public async Task<IActionResult> GetActivePromotionsByShop(Guid shopId, [FromQuery] GetActivePromotionsByShopQuery query)
        {
            query.ShopId = shopId;
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPromotionDetailForBuyer(Guid id, [FromQuery] GetPromotionDetailForBuyerQuery query)
        {
            query.PromotionId = id;
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetApplicablePromotions([FromBody] GetApplicablePromotionsQuery query)
        {
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidatePromotion([FromBody] ValidatePromotionQuery query)
        {
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
