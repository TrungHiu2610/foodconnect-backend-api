using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Complaint.DTOs
{
    public class ComplaintSummaryDto
    {
        public Guid Id { get; set; }
        public OrderComplaintStatusEnum Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        
        public string BuyerReason { get; set; } = string.Empty;
        public bool HasSellerResponse { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? SellerRespondedAt { get; set; }
        public DateTime? AdminDecidedAt { get; set; }
        
        // Order info
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public double OrderTotal { get; set; }
        
        // For buyer view
        public string? ShopName { get; set; }
        
        // For seller view
        public string? BuyerName { get; set; }
    }
}
