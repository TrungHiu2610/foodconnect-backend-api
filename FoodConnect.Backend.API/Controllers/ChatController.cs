using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Chat;
using FoodConnect.Backend.Application.Features.Chat.Commands;
using FoodConnect.Backend.Application.Features.Chat.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ChatController : ApiBaseController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> StartFromOrder([FromBody] StartChatFromOrderCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> StartFromProduct([FromBody] StartChatFromProductCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> StartFromShop([FromBody] StartChatFromShopCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromForm] SendMessageCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> History([FromQuery] GetChatHistoryQuery query)
    {
        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Conversations()
    {
        var query = new GetConversationListQuery();
        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }
}
