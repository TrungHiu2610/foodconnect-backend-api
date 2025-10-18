using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Category.Commands
{
    public class CreateCategoryCommand : IRequest<BaseResponse<CreateCategoryResponse>>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        [JsonIgnore]
        public string? ImageUrl { get; set; }
        public IFormFile? File { get; set; }
        public bool IsActive { get; set; } = true;
        public string? DeliveryType { get; set; } = DeliveryTypeEnum.Standard.ToString();
        public Guid? ParentId { get; set; }
    }
}
