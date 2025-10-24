using FluentValidation;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Commands
{
    public class RejectShopRegistrationCommandValidator : AbstractValidator<RejectShopRegistrationCommand>
    {
        public RejectShopRegistrationCommandValidator()
        {
            RuleFor(x => x.ShopRegistrationId)
                .NotEmpty().WithMessage("Shop registration ID is required.");
            
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required.")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
        }
    }
}
