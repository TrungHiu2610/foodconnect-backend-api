using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class CreateProductCommand : IRequest<BaseResponse<CreateProductResponse>>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Ingredients { get; set; }
        public string Weight { get; set; }
        public string ExpiryDate { get; set; }
        public string StorageInstructions { get; set; }
        public string UsageInstructions { get; set; }
        public string Status { get; set; } = nameof(ProductStatusEnum.Draft);
        public Guid CategoryId { get; set; }

        public bool IsAvailable { get; set; } = true;
        public int? StockQuantity { get; set; }

        public List<ProductAssetCreateDto>? ProductAssets { get; set; }
    }
}
