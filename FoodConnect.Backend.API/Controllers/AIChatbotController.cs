using FoodConnect.Backend.Application.Commons.DTOs.Requests.AIChatbot;
using FoodConnect.Backend.Application.Features.AIChatbot.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AIChatbotController : ApiBaseController
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var command = new SendChatMessageCommand
        {
            Message = request.Message,
            SessionId = request.SessionId
        };

        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateEmbeddings([FromQuery] bool forceRegenerate = false)
    {
        var command = new GenerateProductEmbeddingsCommand
        {
            ForceRegenerate = forceRegenerate
        };

        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }
}
