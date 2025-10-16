using FoodConnect.Backend.Application.Features.Product.Commands;
using FoodConnect.Backend.Application.Features.Product.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductController : ApiBaseController
    {
        [HttpPost]
        public async Task<IActionResult> GetListProducts(GetListProductQuery query)
        {
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id,[FromQuery] GetProductDetailQuery query)
        {
            query.Id = id;
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpPut]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateProduct([FromForm] UpdateProductCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }

        [HttpDelete]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> DeleteProduct(DeleteProductCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
