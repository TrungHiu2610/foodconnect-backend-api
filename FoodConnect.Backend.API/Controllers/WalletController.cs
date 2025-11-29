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
    public async Task<IActionResult> GetWallet([FromQuery] WalletTypeEnum walletType)
    {
        var query = new GetWalletQuery { WalletType = walletType };
        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWallets()
    {
        var result = await Mediator.Send(new GetAllWalletsQuery());
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] WalletTypeEnum walletType, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? transactionType = null)
    {
        var query = new GetNewWalletTransactionsQuery
        {
            WalletType = (int)walletType,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TransactionType = transactionType
        };

        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }
}
