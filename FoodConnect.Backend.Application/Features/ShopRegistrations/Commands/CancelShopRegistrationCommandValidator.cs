using FluentValidation;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Commands
{
    public class CancelShopRegistrationCommandValidator : AbstractValidator<CancelShopRegistrationCommand>
    {
        public CancelShopRegistrationCommandValidator()
        {
            RuleFor(x => x.ShopRegistrationId)
                .NotEmpty().WithMessage("Shop registration ID is required.");
        }
    }
}
