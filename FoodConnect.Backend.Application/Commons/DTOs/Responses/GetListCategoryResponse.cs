using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses
{
    public class GetListCategoryResponse 
    {
        public ICollection<GetListCategoryDetail> Categories { get; set; } = new List<GetListCategoryDetail>();
    }
    public class GetListCategoryDetail
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        //public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public string DeliveryType { get; set; }
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public ICollection<GetListProductDetail>? Products { get; set; }
    }
}
