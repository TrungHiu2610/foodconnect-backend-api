namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Product
{
    public class ProductReviewAssetDto
    {
        public Guid Id { get; set; }
        public string AssetUrl { get; set; } = string.Empty;
        public int AssetType { get; set; } // 0 = Image, 1 = Video
        public string AssetTypeName { get; set; } = string.Empty; // "Image" or "Video"
        public int DisplayOrder { get; set; }
    }
}
