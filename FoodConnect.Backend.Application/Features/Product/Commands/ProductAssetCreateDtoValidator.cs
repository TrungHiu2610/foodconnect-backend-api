using FluentValidation;
using FoodConnect.Backend.Application.Commons.DTOs;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class ProductAssetCreateDtoValidator : AbstractValidator<ProductAssetCreateDto>
    {
        public ProductAssetCreateDtoValidator()
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
