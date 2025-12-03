namespace FoodConnect.Backend.Application.Commons.Helpers
{
    public static class CacheKeys
    {
        public static string SellerDashboard(Guid shopId, DateTime fromDate, DateTime toDate, string period) =>
            $"seller:dashboard:{shopId}:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}:{period}";

        public static string SellerProductStats(Guid shopId, DateTime? fromDate, DateTime? toDate, int topN) =>
            $"seller:products:{shopId}:{fromDate?.ToString("yyyyMMdd") ?? "all"}:{toDate?.ToString("yyyyMMdd") ?? "all"}:{topN}";

        public static string SellerOrderStats(Guid shopId, DateTime fromDate, DateTime toDate) =>
            $"seller:orders:{shopId}:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";

        public static string SystemRevenue(DateTime fromDate, DateTime toDate, Guid? sellerId) =>
            $"admin:revenue:system:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}:{sellerId?.ToString() ?? "all"}";

        public static string RevenueBySeller(DateTime fromDate, DateTime toDate, int topN) =>
            $"admin:revenue:sellers:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}:{topN}";

        public static string RefundStats(DateTime fromDate, DateTime toDate, Guid? sellerId) =>
            $"admin:refund:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}:{sellerId?.ToString() ?? "all"}";

        public static string OrderStatusOverview(DateTime fromDate, DateTime toDate) =>
            $"admin:orders:overview:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";

        public static string CancellationRate(DateTime fromDate, DateTime toDate) =>
            $"admin:cancellation:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";

        public static string TopProducts(DateTime? fromDate, DateTime? toDate, int topN, Guid? categoryId) =>
            $"admin:products:top:{fromDate?.ToString("yyyyMMdd") ?? "all"}:{toDate?.ToString("yyyyMMdd") ?? "all"}:{topN}:{categoryId?.ToString() ?? "all"}";

        public static string NewUserStats(DateTime fromDate, DateTime toDate, string groupBy) =>
            $"admin:users:new:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}:{groupBy}";

        public static string TopSellers(DateTime? fromDate, DateTime? toDate, int topN) =>
            $"admin:sellers:top:{fromDate?.ToString("yyyyMMdd") ?? "all"}:{toDate?.ToString("yyyyMMdd") ?? "all"}:{topN}";

        public static string LoyalCustomers(int topN) =>
            $"admin:customers:loyal:{topN}";

        public static string ComplaintStats(DateTime? fromDate, DateTime? toDate) =>
            $"admin:complaints:{fromDate?.ToString("yyyyMMdd") ?? "all"}:{toDate?.ToString("yyyyMMdd") ?? "all"}";

        public static string SellerHealthScore(Guid? shopId, int topN) =>
            $"admin:health:sellers:{shopId?.ToString() ?? "all"}:{topN}";

        public static string BuyerRiskScore(Guid? buyerId, int topN) =>
            $"admin:risk:buyers:{buyerId?.ToString() ?? "all"}:{topN}";

        public static string BuyerSpending(Guid buyerId, DateTime? fromDate, DateTime? toDate) =>
            $"buyer:spending:{buyerId}:{fromDate?.ToString("yyyyMMdd") ?? "all"}:{toDate?.ToString("yyyyMMdd") ?? "all"}";

        public static string BuyerActivity(Guid buyerId, int topN) =>
            $"buyer:activity:{buyerId}:{topN}";

        public static string AccountViolationList(string? role, decimal? minScore, decimal? maxScore, 
            bool? hasWarning, int pageNumber, int pageSize, string? sortBy, string? sortOrder) =>
            $"admin:violations:list:{role ?? "all"}:{minScore?.ToString() ?? "nomin"}:{maxScore?.ToString() ?? "nomax"}:{hasWarning?.ToString() ?? "all"}:{pageNumber}:{pageSize}:{sortBy ?? "score"}:{sortOrder ?? "desc"}";

        public static string AccountActivityDetail(Guid userId, int recentOrdersLimit) =>
            $"admin:violations:detail:{userId}:{recentOrdersLimit}";

        public static string SellerPattern(Guid shopId) => $"seller:*:{shopId}*";
        public static string AdminPattern() => "admin:*";
        public static string BuyerPattern(Guid buyerId) => $"buyer:*:{buyerId}*";

        public static class Expiration
        {
            public static TimeSpan SellerDashboard = TimeSpan.FromMinutes(5);
            public static TimeSpan SellerStats = TimeSpan.FromMinutes(10);
            public static TimeSpan AdminFinancial = TimeSpan.FromMinutes(15);
            public static TimeSpan AdminOperational = TimeSpan.FromMinutes(10);
            public static TimeSpan AdminUserAnalytics = TimeSpan.FromHours(1);
            public static TimeSpan AdminQuality = TimeSpan.FromMinutes(30);
            public static TimeSpan BuyerStats = TimeSpan.FromMinutes(15);
        }
    }
}
