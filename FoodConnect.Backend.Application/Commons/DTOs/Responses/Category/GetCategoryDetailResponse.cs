using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Category
{
    public class GetCategoryDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public string DeliveryType { get; set; }
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public ICollection<GetListProductItemResponse>? Products { get; set; }
    }
}
