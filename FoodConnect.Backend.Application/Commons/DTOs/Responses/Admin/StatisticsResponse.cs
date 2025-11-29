namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin
{
    // A. Financial Analytics
    public class SystemRevenueResponse
    {
        public decimal GMV { get; set; }
        public decimal PlatformRevenue { get; set; }
        public decimal PlatformFeePercentage { get; set; }
        public int TotalCompletedOrders { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ComparisonData GMVComparison { get; set; } = new();
        public ComparisonData RevenueComparison { get; set; } = new();
    }

    public class ComparisonData
    {
        public decimal PreviousPeriodValue { get; set; }
        public decimal CurrentPeriodValue { get; set; }
        public decimal AbsoluteDelta { get; set; }
        public decimal PercentageChange { get; set; }
        public bool IsIncreased { get; set; }
    }

    public class RevenueBySellerResponse
    {
        public List<SellerRevenueItem> SellerRevenues { get; set; } = new();
        public decimal TotalGMV { get; set; }
        public decimal TotalPlatformFee { get; set; }
    }

    public class SellerRevenueItem
    {
        public Guid SellerId { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal PlatformFee { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class RefundStatisticsResponse
    {
        public decimal TotalRefundAmount { get; set; }
        public int TotalRefundCount { get; set; }
        public decimal RefundRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<RefundBySellerItem> RefundsBySeller { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class RefundBySellerItem
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public int RefundCount { get; set; }
        public decimal RefundRate { get; set; }
    }

    // B. Operational Analytics
    public class OrderStatusOverviewResponse
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int RefundedOrders { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal CancellationRate { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class CancellationRateResponse
    {
        public decimal OverallCancellationRate { get; set; }
        public int TotalOrders { get; set; }
        public int CancelledOrders { get; set; }
        public List<CancellationBySellerItem> CancellationsBySeller { get; set; } = new();
        public ComparisonData MonthComparison { get; set; } = new();
    }

    public class CancellationBySellerItem
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal CancellationRate { get; set; }
    }

    public class TopProductsResponse
    {
        public List<TopProductItem> GlobalBestSellers { get; set; } = new();
        public List<CategoryBestSeller> BestSellersByCategory { get; set; } = new();
    }

    public class TopProductItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class CategoryBestSeller
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<TopProductItem> Products { get; set; } = new();
    }

    // C. User Analytics
    public class NewUserStatisticsResponse
    {
        public int NewUsersCount { get; set; }
        public decimal GrowthRate { get; set; }
        public int PreviousPeriodCount { get; set; }
        public List<UserGrowthTimeSeries> TimeSeries { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class UserGrowthTimeSeries
    {
        public DateTime Date { get; set; }
        public int UserCount { get; set; }
        public int CumulativeCount { get; set; }
    }

    public class TopSellersResponse
    {
        public List<TopSellerItem> TopSellersByRevenue { get; set; } = new();
        public List<TopSellerItem> TopSellersByOrders { get; set; } = new();
        public List<TopSellerItem> TopSellersByRating { get; set; } = new();
    }

    public class TopSellerItem
    {
        public Guid SellerId { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public decimal? Rating { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int ReviewCount { get; set; }
    }

    public class LoyalCustomersResponse
    {
        public List<LoyalCustomerItem> TopCustomersByOrders { get; set; } = new();
        public List<LoyalCustomerItem> TopCustomersBySpending { get; set; } = new();
    }

    public class LoyalCustomerItem
    {
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpending { get; set; }
        public DateTime FirstOrderDate { get; set; }
        public DateTime LastOrderDate { get; set; }
    }

    // D. Quality Analytics
    public class ComplaintStatisticsResponse
    {
        public int TotalComplaints { get; set; }
        public int TotalOrders { get; set; }
        public decimal ComplaintRate { get; set; }
        public List<ComplaintBySellerItem> ComplaintsBySeller { get; set; } = new();
        public Dictionary<string, int> ComplaintStatusBreakdown { get; set; } = new();
    }

    public class ComplaintBySellerItem
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int ComplaintCount { get; set; }
        public int TotalOrders { get; set; }
        public decimal ComplaintRate { get; set; }
    }

    public class SellerHealthScoreResponse
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public decimal HealthScore { get; set; }
        public HealthScoreBreakdown Breakdown { get; set; } = new();
        public string HealthStatus { get; set; } = string.Empty;
    }

    public class HealthScoreBreakdown
    {
        public decimal CompletionRateScore { get; set; }
        public decimal CancellationRateScore { get; set; }
        public decimal ComplaintRateScore { get; set; }
        public decimal ProcessingSpeedScore { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal CancellationRate { get; set; }
        public decimal ComplaintRate { get; set; }
        public double AverageProcessingHours { get; set; }
    }

    public class BuyerRiskScoreResponse
    {
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public decimal RiskScore { get; set; }
        public RiskScoreBreakdown Breakdown { get; set; } = new();
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class RiskScoreBreakdown
    {
        public int RefundCount { get; set; }
        public int ComplaintCount { get; set; }
        public int CancellationCount { get; set; }
        public int TotalOrders { get; set; }
        public decimal RefundRate { get; set; }
        public decimal ComplaintRate { get; set; }
        public decimal CancellationRate { get; set; }
    }
}
