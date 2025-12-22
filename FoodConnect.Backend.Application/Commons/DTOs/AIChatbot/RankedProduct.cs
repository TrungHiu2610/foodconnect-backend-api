namespace FoodConnect.Backend.Application.Commons.DTOs.AIChatbot;

public class RankedProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Ingredients { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public decimal? ShopRating { get; set; }
    public string? ThumbnailUrl { get; set; }
    public double Score { get; set; }
    public string? Reason { get; set; }
}
