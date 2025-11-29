using FoodConnect.Backend.Application.Features.Buyer.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Buyer")]
    public class BuyerStatisticsController : ApiBaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetSpendingStatistics(
            [FromQuery] Guid buyerId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var query = new GetBuyerSpendingStatisticsQuery
            {
                BuyerId = buyerId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderActivity(
            [FromQuery] Guid buyerId,
            [FromQuery] int topProductsCount = 10)
        {
            var query = new GetBuyerOrderActivityQuery
            {
                BuyerId = buyerId,
                TopProductsCount = topProductsCount
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
