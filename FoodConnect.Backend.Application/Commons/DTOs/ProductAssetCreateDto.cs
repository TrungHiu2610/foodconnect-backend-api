using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace FoodConnect.Backend.Application.Commons.DTOs
{
    public class ProductAssetCreateDto
    {
        [JsonIgnore]
        public string? AssetName { get; set; }
        public string? AssetDescription { get; set; }
        [JsonIgnore]
        public string? AssetUrl { get; set; }
        public bool IsThumbnail { get; set; }
        public IFormFile? File { get; set; }
    }
}
