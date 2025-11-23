namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Seller
{
    public class SellerDashboardResponse
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProductsSold { get; set; }
        public ComparisonData RevenueComparison { get; set; } = new();
        public ComparisonData OrdersComparison { get; set; } = new();
        public ComparisonData ProductsSoldComparison { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Period { get; set; } = string.Empty;
    }

    public class ComparisonData
    {
        public decimal PreviousPeriodValue { get; set; }
        public decimal CurrentPeriodValue { get; set; }
        public decimal AbsoluteDelta { get; set; }
        public decimal PercentageChange { get; set; }
        public bool IsIncreased { get; set; }
    }

    public class SellerProductStatisticsResponse
    {
        public List<ProductSalesItem> BestSellingProducts { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int TotalQuantitySold { get; set; }
    }

    public class ProductSalesItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ContributionPercentage { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class SellerOrderStatisticsResponse
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal CancellationRate { get; set; }
        public List<OrderDetailItem> Orders { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class OrderDetailItem
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public List<OrderProductItem> Products { get; set; } = new();
    }

    public class OrderProductItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
