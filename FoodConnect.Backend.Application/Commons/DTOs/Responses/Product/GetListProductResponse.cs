using FoodConnect.Backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Product
{
    public class GetListProductResponse
    {
        public ICollection<GetListProductItemResponse> Products { get; set; } = new List<GetListProductItemResponse>();
    }
    public class GetListProductItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Unit { get; set; }
        public string? Ingredients { get; set; }
        public string? Weight { get; set; }
        public string Status { get; set; }
        public bool IsAvailable { get; set; }
        public int? StockQuantity { get; set; } 
        
        //public string ShopName { get; set; }
        //public decimal? Rating { get; set; }
    }
}
