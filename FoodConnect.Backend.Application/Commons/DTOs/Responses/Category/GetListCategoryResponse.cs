using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Category
{
    public class GetListCategoryResponse
    {
        public ICollection<GetListCategoryItem> Categories { get; set; } = new List<GetListCategoryItem>();
    }
    public class GetListCategoryItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
    }
}
