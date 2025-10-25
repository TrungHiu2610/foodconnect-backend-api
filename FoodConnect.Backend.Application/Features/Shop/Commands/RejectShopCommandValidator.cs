using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class RejectShopCommandValidator : AbstractValidator<RejectShopCommand>
    {
        public RejectShopCommandValidator()
        {
            RuleFor(x => x.ShopId)
                .NotEmpty().WithMessage("Shop ID is required");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Rejection reason is required")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
        }
    }
}
