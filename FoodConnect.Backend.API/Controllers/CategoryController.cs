using FoodConnect.Backend.Application.Features.Category.Commands;
using FoodConnect.Backend.Application.Features.Category.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CategoryController : ApiBaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetListCategories([FromQuery] GetListCategoryQuery query)
        {
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromForm] CreateCategoryCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory([FromForm] UpdateCategoryCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpDelete]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> DeleteCategory(DeleteCategoryCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
