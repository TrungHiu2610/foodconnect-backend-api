using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class RejectPromotionCommandValidator : AbstractValidator<RejectPromotionCommand>
    {
        public RejectPromotionCommandValidator()
        {
            RuleFor(x => x.PromotionId)
                .NotEmpty().WithMessage("Promotion ID is required");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Rejection reason is required")
                .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters");
        }
    }
}
