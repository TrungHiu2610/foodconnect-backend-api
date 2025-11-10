using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class CreateProductReviewCommandValidator : AbstractValidator<CreateProductReviewCommand>
    {
        public CreateProductReviewCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order ID is required");

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5 stars");

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Comment));

            When(x => x.ReviewImage != null, () =>
            {
                RuleFor(x => x.ReviewImage!.Length)
                    .LessThanOrEqualTo(10 * 1024 * 1024)
                    .WithMessage("Review image must not exceed 10MB");

                RuleFor(x => x.ReviewImage!.ContentType)
                    .Must(contentType => contentType.StartsWith("image/"))
                    .WithMessage("File must be an image");
            });
        }
    }
}
