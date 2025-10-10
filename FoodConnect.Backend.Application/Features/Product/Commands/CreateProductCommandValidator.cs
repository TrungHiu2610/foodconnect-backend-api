using FluentValidation;
using FoodConnect.Backend.Application.Commons.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit is required.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");

            When(x => x.ProductAssets != null && x.ProductAssets.Any(), () =>
            {
                RuleForEach(x => x.ProductAssets).SetValidator(new ProductAssetDtoValidator());
            });
        }
    }

    public class ProductAssetDtoValidator : AbstractValidator<ProductAssetDto>
    {
        public ProductAssetDtoValidator()
        {
            When(x => x.File != null, () =>
            {
                RuleFor(x => x.File.Length)
                    .LessThanOrEqualTo(100 * 1024 * 1024) // 100MB
                    .WithMessage("File size must be less than or equal to 100MB.");

                RuleFor(x => x.File.ContentType.ToLower())
                    .Must(ct => ct.StartsWith("image") || ct.StartsWith("video"))
                    .WithMessage("File must be image or video.");
            });
        }
    }
}
