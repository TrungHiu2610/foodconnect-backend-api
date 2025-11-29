namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin
{
    public class AccountViolationListResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public int UserStatus { get; set; }
        public string UserStatusName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        
        public SellerViolationMetrics? SellerMetrics { get; set; }
        
        public BuyerViolationMetrics? BuyerMetrics { get; set; }
        
        public bool HasWarningBadge { get; set; }
        public string? WarningReason { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }

    public class SellerViolationMetrics
    {
        public Guid? ShopId { get; set; }
        public string? ShopName { get; set; }
        public decimal HealthScore { get; set; }
        public string HealthStatus { get; set; } = string.Empty;
        public decimal CancellationRate { get; set; }
        public decimal ComplaintRate { get; set; }
        public decimal CompletionRate { get; set; }
        public int TotalOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ComplaintOrders { get; set; }
    }

    public class BuyerViolationMetrics
    {
        public decimal RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public decimal CancellationRate { get; set; }
        public decimal RefundRate { get; set; }
        public decimal ComplaintRate { get; set; }
        public int TotalOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int RefundOrders { get; set; }
        public int ComplaintOrders { get; set; }
    }

    public class AccountActivityDetailResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int UserStatus { get; set; }
        public string UserStatusName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        
        public List<RecentOrderActivity> RecentOrders { get; set; } = new();
        public AccountStatisticsSummary Statistics { get; set; } = new();
        
        public List<ViolationHistoryItem> ViolationHistory { get; set; } = new();
    }

    public class RecentOrderActivity
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int OrderStatus { get; set; }
        public string OrderStatusName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
        
        public string? BuyerName { get; set; }
        
        public string? ShopName { get; set; }
        public Guid? ShopId { get; set; }
        
        public bool IsViolation { get; set; }
        public string? ViolationType { get; set; }
    }

    public class AccountStatisticsSummary
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int RefundedOrders { get; set; }
        public decimal TotalSpent { get; set; } // For buyer
        public decimal TotalRevenue { get; set; } // For seller
        public decimal AverageOrderValue { get; set; }
        public DateTime? FirstOrderDate { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int DaysSinceFirstOrder { get; set; }
    }

    public class ViolationHistoryItem
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string ViolationType { get; set; } = string.Empty; // "Cancelled", "Returned", "Complaint"
        public string? Reason { get; set; }
        public DateTime ViolationDate { get; set; }
        public decimal OrderAmount { get; set; }
    }

    public class AccountLockResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int PreviousStatus { get; set; }
        public int NewStatus { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
    }
}
