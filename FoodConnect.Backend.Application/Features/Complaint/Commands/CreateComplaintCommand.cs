using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Complaint.Commands
{
    public class CreateComplaintCommand : IRequest<BaseResponse<ComplaintDetailDto>>
    {
        public Guid OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<IFormFile>? EvidenceFiles { get; set; }
    }
}
