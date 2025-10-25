using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetListShopsQueryValidator : AbstractValidator<GetListShopsQuery>
    {
        public GetListShopsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
        }
    }
}
