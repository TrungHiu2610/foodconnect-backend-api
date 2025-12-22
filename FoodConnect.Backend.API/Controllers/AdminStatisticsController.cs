using FoodConnect.Backend.Application.Features.Admin.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminStatisticsController : ApiBaseController
    {

        [HttpGet]
        public async Task<IActionResult> GetSystemRevenue(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] Guid? sellerId = null)
        {
            var query = new GetSystemRevenueQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                SellerId = sellerId
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueBySeller(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int topN = 20)
        {
            var query = new GetRevenueBySellerQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                TopN = topN
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRefundStatistics(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] Guid? sellerId = null)
        {
            var query = new GetRefundStatisticsQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                SellerId = sellerId
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetOrderStatusOverview(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var query = new GetOrderStatusOverviewQuery
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCancellationRate(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var query = new GetCancellationRateQuery
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopProducts(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int topN = 10,
            [FromQuery] Guid? categoryId = null)
        {
            var query = new GetTopProductsQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                TopN = topN,
                CategoryId = categoryId
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetNewUserStatistics(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] string groupBy = "month")
        {
            var query = new GetNewUserStatisticsQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                GroupBy = groupBy
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopSellers(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int topN = 10)
        {
            var query = new GetTopSellersQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                TopN = topN
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetLoyalCustomers([FromQuery] int topN = 10)
        {
            var query = new GetLoyalCustomersQuery { TopN = topN };
            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetComplaintStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var query = new GetComplaintStatisticsQuery
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSellerHealthScore(
            [FromQuery] Guid? shopId = null,
            [FromQuery] int topN = 20)
        {
            var query = new GetSellerHealthScoreQuery
            {
                ShopId = shopId,
                TopN = topN
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBuyerRiskScore(
            [FromQuery] Guid? buyerId = null,
            [FromQuery] int topN = 20)
        {
            var query = new GetBuyerRiskScoreQuery
            {
                BuyerId = buyerId,
                TopN = topN
            };

            var result = await Mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
