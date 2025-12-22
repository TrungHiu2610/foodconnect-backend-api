using FoodConnect.Backend.Application.Features.Seller.Queries;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Seller")]
    public class SellerStatisticsController : ApiBaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] Guid shopId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] string period = "month")
        {
            var query = new GetSellerDashboardQuery
            {
                ShopId = shopId,
                FromDate = fromDate,
                ToDate = toDate,
                Period = period
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductStatistics(
            [FromQuery] Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int topN = 10)
        {
            var query = new GetSellerProductStatisticsQuery
            {
                ShopId = shopId,
                FromDate = fromDate,
                ToDate = toDate,
                TopN = topN
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderStatistics(
            [FromQuery] Guid shopId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] OrderStatusEnum? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetSellerOrderStatisticsQuery
            {
                ShopId = shopId,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
