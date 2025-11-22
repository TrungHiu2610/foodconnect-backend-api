using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Complaint.DTOs
{
    public class ComplaintDetailDto
    {
        public Guid Id { get; set; }
        public OrderComplaintStatusEnum Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        
        // Buyer information
        public string BuyerReason { get; set; } = string.Empty;
        public List<ComplaintAssetDto> BuyerEvidences { get; set; } = new List<ComplaintAssetDto>();
        
        // Seller response
        public string? SellerResponse { get; set; }
        public DateTime? SellerRespondedAt { get; set; }
        public decimal? SellerDesiredRefundPercentage { get; set; }
        public decimal? SellerDesiredRefundAmount { get; set; }
        public bool IsSellerRefundFixedAmount { get; set; }
        public List<ComplaintAssetDto> SellerEvidences { get; set; } = new List<ComplaintAssetDto>();
        
        // Admin decision
        public string? AdminDecisionReason { get; set; }
        public DateTime? AdminDecidedAt { get; set; }
        public decimal? ApprovedRefundAmount { get; set; }
        public bool IsApproved { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // Order information
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public double OrderTotal { get; set; }
        
        // Buyer info
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        
        // Seller info
        public Guid SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        
        // Admin info
        public Guid? AdminId { get; set; }
        public string? AdminName { get; set; }
    }
}
