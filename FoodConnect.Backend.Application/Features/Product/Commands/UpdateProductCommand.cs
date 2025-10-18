using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using System.Text.Json.Serialization;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class UpdateProductCommand : IRequest<BaseResponse<UpdateProductResponse>>
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Unit { get; set; }
        public string? Status { get; set; } = nameof(ProductStatusEnum.Draft);
        public Guid? CategoryId { get; set; }

        public List<ProductAssetCreateDto>? NewProductAssets { get; set; }
        public List<Guid>? RemovedProductAssetIds { get; set; }
        public List<ProductAssetUpdateDto>? UpdateProductAssets { get; set; }
    }
}
