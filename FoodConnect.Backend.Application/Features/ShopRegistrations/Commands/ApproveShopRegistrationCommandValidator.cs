using FluentValidation;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Commands
{
    public class ApproveShopRegistrationCommandValidator : AbstractValidator<ApproveShopRegistrationCommand>
    {
        public ApproveShopRegistrationCommandValidator()
        {
            RuleFor(x => x.ShopRegistrationId)
                .NotEmpty().WithMessage("Shop registration ID is required.");
        }
    }
}
