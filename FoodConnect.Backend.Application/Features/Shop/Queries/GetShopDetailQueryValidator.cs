using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetShopDetailQueryValidator : AbstractValidator<GetShopDetailQuery>
    {
        public GetShopDetailQueryValidator()
        {
            RuleFor(x => x.ShopId)
                .NotEmpty().WithMessage("Shop ID is required");
        }
    }
}
