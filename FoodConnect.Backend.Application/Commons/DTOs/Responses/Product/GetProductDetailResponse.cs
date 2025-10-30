using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Product
{
    public class GetProductDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public string? Ingredients { get; set; }
        public string? Weight { get; set; }
        public string Status { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string DeliveryType { get; set; }
        public bool IsAvailable { get; set; }
        public int? StockQuantity { get; set; }
        
        public ICollection<ProductAssetGetDto> ProductAssets { get; set; } = new List<ProductAssetGetDto>();
    }
}
