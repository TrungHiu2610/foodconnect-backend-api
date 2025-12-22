using FoodConnect.Backend.Application.Features.Complaint.Commands;
using FoodConnect.Backend.Application.Features.Complaint.Commands.ApproveComplaint;
using FoodConnect.Backend.Application.Features.Complaint.Commands.RejectComplaint;
using FoodConnect.Backend.Application.Features.Complaint.Queries;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodConnect.Backend.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ComplaintController : ApiBaseController
    {
        [HttpPost]
        public async Task<IActionResult> CreateComplaint([FromForm] CreateComplaintCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpGet]
        public async Task<IActionResult> GetMyComplaints([FromQuery] OrderComplaintStatusEnum? status = null)
        {
            var query = new GetMyComplaintsQuery { Status = status };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpGet]
        public async Task<IActionResult> GetComplaintDetail([FromQuery] Guid complaintId)
        {
            var query = new GetComplaintDetailQuery { ComplaintId = complaintId };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> RespondToComplaint([FromForm] RespondToComplaintCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpGet]
        public async Task<IActionResult> GetShopComplaints([FromQuery] Guid shopId, [FromQuery] OrderComplaintStatusEnum? status = null)
        {
            var query = new GetShopComplaintsQuery 
            { 
                ShopId = shopId,
                Status = status 
            };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllComplaints([FromQuery] OrderComplaintStatusEnum? status = null)
        {
            var query = new GetAllComplaintsQuery { Status = status };
            var result = await Mediator.Send(query);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> ApproveComplaint([FromBody] ApproveComplaintCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> RejectComplaint([FromBody] RejectComplaintCommand command)
        {
            var result = await Mediator.Send(command);
            return result != null ? (result.Success ? Ok(result) : BadRequest(result)) : BadRequest();
        }
    }
}
