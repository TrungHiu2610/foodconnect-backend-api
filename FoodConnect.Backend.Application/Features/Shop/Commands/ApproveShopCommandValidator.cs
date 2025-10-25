using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class ApproveShopCommandValidator : AbstractValidator<ApproveShopCommand>
    {
        public ApproveShopCommandValidator()
        {
            RuleFor(x => x.ShopId)
                .NotEmpty().WithMessage("Shop ID is required");
        }
    }
}
