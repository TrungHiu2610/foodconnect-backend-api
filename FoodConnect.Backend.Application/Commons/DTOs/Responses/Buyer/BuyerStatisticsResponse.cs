namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Buyer
{
    public class BuyerSpendingStatisticsResponse
    {
        public decimal TotalSpending { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<MonthlySpendingItem> MonthlySpending { get; set; } = new();
        public List<SpendingByShopItem> SpendingByShop { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class MonthlySpendingItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }
        public decimal CumulativeAmount { get; set; }
    }

    public class SpendingByShopItem
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    public class BuyerOrderActivityResponse
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        public int RefundedOrders { get; set; }
        public decimal CompletionRate { get; set; }
        public Dictionary<string, int> OrderStatusBreakdown { get; set; } = new();
        public List<TopPurchasedProductItem> TopPurchasedProducts { get; set; } = new();
        public List<InteractedSellerItem> InteractedSellers { get; set; } = new();
        public DateTime? FirstOrderDate { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }

    public class TopPurchasedProductItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
    }

    public class InteractedSellerItem
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime FirstOrderDate { get; set; }
        public DateTime LastOrderDate { get; set; }
    }
}
