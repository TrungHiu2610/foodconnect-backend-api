using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Complaint.DTOs
{
    public class RespondToComplaintDto
    {
        public Guid ComplaintId { get; set; }
        public string Response { get; set; } = string.Empty;
        
        // Refund suggestion
        public bool IsFixedAmount { get; set; } = false;
        public decimal? RefundPercentage { get; set; }  // 0-100 if percentage
        public decimal? RefundAmount { get; set; }      // Fixed amount
        
        public List<IFormFile>? EvidenceFiles { get; set; }
    }
}
