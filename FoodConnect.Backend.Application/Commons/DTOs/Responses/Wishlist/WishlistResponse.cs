namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Wishlist
{
    public class WishlistResponse
    {
        public Guid Id { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public bool NotificationEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public WishlistItemDto? Item { get; set; }
    }

    public class WishlistItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public bool? IsAvailable { get; set; }
        public string? Status { get; set; }
        public decimal? Rating { get; set; }
        public int? ReviewCount { get; set; }
    }
}
