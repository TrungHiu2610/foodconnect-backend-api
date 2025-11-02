using FoodConnect.Backend.Application.Features.Address.Commands;
using FoodConnect.Backend.Application.Features.Address.Queries;
using FoodConnect.Backend.Application.Features.User.Commands;
using FoodConnect.Backend.Application.Features.User.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UserController : ApiBaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var query = new GetUserProfileQuery();
            var result = await Mediator.Send(query);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress([FromRoute] Guid id)
        {
            var command = new DeleteAddressCommand { Id = id };
            var result = await Mediator.Send(command);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            var query = new GetMyAddressesQuery();
            var result = await Mediator.Send(query);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> SetDefaultAddress([FromBody] SetDefaultAddressCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null
                ? (result.Success ? Ok(result) : BadRequest(result))
                : BadRequest();
        }
    }
}
