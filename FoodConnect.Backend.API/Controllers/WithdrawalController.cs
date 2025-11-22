using FoodConnect.Backend.Application.Features.Withdrawal.Commands;
using FoodConnect.Backend.Application.Features.Withdrawal.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class WithdrawalController : ApiBaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateWithdrawalRequestCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory([FromQuery] int? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetWithdrawalHistoryQuery
        {
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpGet("{requestId}")]
    public async Task<IActionResult> GetMyDetail([FromRoute] Guid requestId)
    {
        var query = new GetMyWithdrawalDetailQuery
        {
            RequestId = requestId
        };

        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpPost("{withdrawalRequestId}")]
    public async Task<IActionResult> CancelRequest([FromRoute] Guid withdrawalRequestId)
    {
        var command = new CancelWithdrawalRequestCommand
        {
            WithdrawalRequestId = withdrawalRequestId
        };

        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpPost]
    public async Task<IActionResult> ReportIssue([FromForm] ReportWithdrawalIssueCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetListRequests(
        [FromQuery] int? status,
        [FromQuery] Guid? sellerId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetWithdrawalRequestsQuery
        {
            Status = status,
            SellerId = sellerId,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpGet("{requestId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDetailRequest([FromRoute] Guid requestId)
    {
        var query = new GetWithdrawalRequestDetailQuery
        {
            RequestId = requestId
        };

        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProcessRequest([FromForm] ProcessWithdrawalRequestCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResolveIssue([FromForm] ResolveWithdrawalIssueCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }
}
