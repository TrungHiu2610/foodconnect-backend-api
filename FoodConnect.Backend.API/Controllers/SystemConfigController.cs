using FoodConnect.Backend.Application.Features.SystemConfig.Commands;
using FoodConnect.Backend.Application.Features.SystemConfig.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SystemConfigController : ApiBaseController
{
    [HttpGet]
    //[Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> GetConfigs([FromQuery] string? configKey = null, [FromQuery] int? type = null)
    {
        var query = new GetSystemConfigsQuery
        {
            ConfigKey = configKey,
            Type = type
        };

        var result = await Mediator.Send(query);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateConfig([FromForm] CreateSystemConfigCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateConfig([FromForm] UpdateSystemConfigCommand command)
    {
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfig([FromQuery] Guid id)
    {
        var command = new DeleteSystemConfigCommand { Id = id };
        var result = await Mediator.Send(command);
        return result != null
            ? (result.Success ? Ok(result) : BadRequest(result))
            : BadRequest();
    }
}
