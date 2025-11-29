using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Complaint.DTOs
{
    public class CreateComplaintDto
    {
        public Guid OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<IFormFile>? EvidenceFiles { get; set; }
    }
}
