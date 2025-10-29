using FluentValidation;
using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateProductCommandValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit is required.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");

            // Inventory validation based on DeliveryType
            RuleFor(x => x)
                .MustAsync(async (command, cancellation) =>
                {
                    var category = await _categoryRepository.GetByIdAsync(command.CategoryId);
                    if (category == null) return true; // Let other validation handle missing category

                    // Standard (packaged) products require StockQuantity
                    if (category.DeliveryType == DeliveryTypeEnum.Standard)
                    {
                        return command.StockQuantity.HasValue;
                    }

                    return true; // Express products don't require stock quantity
                })
                .WithMessage("Standard delivery products require Stock Quantity.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .When(x => x.StockQuantity.HasValue)
                .WithMessage("Stock Quantity must be greater than or equal to 0.");

            When(x => x.ProductAssets != null && x.ProductAssets.Any(), () =>
            {
                RuleForEach(x => x.ProductAssets).SetValidator(new CreateProductAssetDtoValidator());
            });
        }
    }
}
