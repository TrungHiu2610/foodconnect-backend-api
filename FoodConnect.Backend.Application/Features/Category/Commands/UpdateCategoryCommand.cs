using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Category.Commands
{
    public class UpdateCategoryCommand : IRequest<BaseResponse<UpdateCategoryResponse>>
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
        public bool? IsActive { get; set; } = true;
        public string? DeliveryType { get; set; }
        public Guid? ParentId { get; set; }
    }
}