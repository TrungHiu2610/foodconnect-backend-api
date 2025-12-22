namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.AIChatbot;

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public List<ProductRecommendation> RecommendedProducts { get; set; } = new();
    public List<string> SuggestedQuestions { get; set; } = new();
    public string? SessionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ProductRecommendation
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string ProductUrl { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string ShopUrl { get; set; } = string.Empty;
    public decimal? ShopRating { get; set; }
    
    public string? Reason { get; set; }
    public double? RelevanceScore { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    public int? StockQuantity { get; set; }
}
