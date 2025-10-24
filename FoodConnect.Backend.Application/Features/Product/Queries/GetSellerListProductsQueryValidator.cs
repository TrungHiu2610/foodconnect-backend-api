using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetSellerListProductsQueryValidator : AbstractValidator<GetSellerListProductsQuery>
    {
        public GetSellerListProductsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");
        }
    }
}
