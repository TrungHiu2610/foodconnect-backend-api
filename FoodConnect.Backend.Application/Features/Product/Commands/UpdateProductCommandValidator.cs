using FluentValidation;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public UpdateProductCommandValidator(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;

            RuleFor(x => x.Name)
                    .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");
            
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.Status)
                .Must(status => Enum.TryParse(typeof(Domain.Enums.ProductStatusEnum), status, true, out _))
                .WithMessage("Status must be a valid ProductStatusEnum value.");

            RuleFor(x => x)
                .MustAsync(async (command, cancellation) =>
                {
                    var product = await _productRepository.GetByIdAsync(command.Id);
                    if (product == null) return true; // Let other validation handle missing product

                    var category = await _categoryRepository.GetByIdAsync(command.CategoryId ?? product.CategoryId);
                    if (category == null) return true;

                    if (category.DeliveryType == DeliveryTypeEnum.Standard)
                    {
                        if (command.StockQuantity.HasValue)
                        {
                            return command.StockQuantity.Value >= 0;
                        }
                    }

                    return true;
                })
                .WithMessage("Standard delivery products require valid Stock Quantity (>= 0).");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .When(x => x.StockQuantity.HasValue)
                .WithMessage("Stock Quantity must be greater than or equal to 0.");

            RuleFor(x => x.ExpiryDate)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.ExpiryDate))
                .WithMessage("Expiry Date must not exceed 50 characters.");

            RuleFor(x => x.StorageInstructions)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.StorageInstructions))
                .WithMessage("Storage Instructions must not exceed 500 characters.");

            RuleFor(x => x.UsageInstructions)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.UsageInstructions))
                .WithMessage("Usage Instructions must not exceed 1000 characters.");

            When(x => x.NewProductAssets != null && x.NewProductAssets.Any(), () =>
            {
                RuleForEach(x => x.NewProductAssets).SetValidator(new CreateProductAssetDtoValidator());
            });

            When(x => x.UpdateProductAssets != null && x.UpdateProductAssets.Any(), () =>
            {
                RuleForEach(x => x.UpdateProductAssets).SetValidator(new UpdateProductAssetDtoValidator());
            });
        }
    }
}
