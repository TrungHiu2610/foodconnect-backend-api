using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class SellerRespondToReviewCommandValidator : AbstractValidator<SellerRespondToReviewCommand>
    {
        public SellerRespondToReviewCommandValidator()
        {
            RuleFor(x => x.ReviewId)
                .NotEmpty().WithMessage("Review ID is required");

            RuleFor(x => x.SellerResponse)
                .NotEmpty().WithMessage("Response is required")
                .MaximumLength(1000).WithMessage("Response must not exceed 1000 characters");
        }
    }
}
