using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Complaint.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Complaint.Commands
{
    public class RespondToComplaintCommand : IRequest<BaseResponse<ComplaintDetailDto>>
    {
        public Guid ComplaintId { get; set; }
        public string Response { get; set; } = string.Empty;
        
        public bool IsFixedAmount { get; set; } = false;
        public decimal? RefundPercentage { get; set; }
        public decimal? RefundAmount { get; set; }
        
        public List<IFormFile>? EvidenceFiles { get; set; }
    }
}
