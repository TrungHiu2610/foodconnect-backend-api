using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class SubmitShopCommandValidator : AbstractValidator<SubmitShopCommand>
    {
        public SubmitShopCommandValidator()
        {
            RuleFor(x => x.ShopId)
                .NotEmpty().WithMessage("Shop ID is required");
        }
    }
}
