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
        public ICollection<GetListProductDetail> Products { get; set; } = new List<GetListProductDetail>();
    }
    public class GetListProductDetail
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public ICollection<ProductAssetGetDto> ProductAssets { get; set; } = new List<ProductAssetGetDto>();
        public ICollection<ProductDailyAvailability> ProductDailyAvailabilities { get; set; } = new List<ProductDailyAvailability>();
    }
}
