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
    /// <summary>
    /// Start chat from order page - creates conversation and system message
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> StartFromOrder([FromBody] StartChatFromOrderCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    /// <summary>
    /// Start chat from product page - creates conversation and system message
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> StartFromProduct([FromBody] StartChatFromProductCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    /// <summary>
    /// Start chat from shop page - creates conversation without system message
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> StartFromShop([FromBody] StartChatFromShopCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    /// <summary>
    /// Send a message (text, image, or video)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromForm] SendMessageCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    /// <summary>
    /// Get chat history with pagination
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> History([FromQuery] GetChatHistoryQuery query)
    {
        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    /// <summary>
    /// Get list of conversations for current user
    /// </summary>
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
