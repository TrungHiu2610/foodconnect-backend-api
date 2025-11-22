using FoodConnect.Backend.Application.Features.Wallet.Queries;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class WalletController : ApiBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetMyWallet()
    {
        var result = await Mediator.Send(new GetSellerWalletQuery());
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] TransactionTypeEnum? type = null)
    {
        var query = new GetWalletTransactionsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Type = type
        };

        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }
}
