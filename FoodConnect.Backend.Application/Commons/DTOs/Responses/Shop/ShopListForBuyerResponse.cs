namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop
{
    public class ShopListForBuyerResponse
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public List<string> MainCategories { get; set; } = new List<string>();
        public decimal? Rating { get; set; }
        public int ReviewCount { get; set; }
        public int TotalOrders { get; set; }
        public double? Distance { get; set; }
        public bool IsOpen { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsVerified { get; set; }
        public List<string> Badges { get; set; } = new List<string>();
        public string Address { get; set; } = string.Empty;
    }
}
