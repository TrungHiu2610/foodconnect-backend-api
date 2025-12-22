using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Wishlist.Commands
{
    public class AddToWishlistCommandValidator : AbstractValidator<AddToWishlistCommand>
    {
        public AddToWishlistCommandValidator()
        {
            RuleFor(x => x)
                .Must(x => x.ProductId.HasValue || x.ShopId.HasValue)
                .WithMessage("Either ProductId or ShopId must be provided");

            RuleFor(x => x)
                .Must(x => !(x.ProductId.HasValue && x.ShopId.HasValue))
                .WithMessage("Cannot add both Product and Shop at the same time");
        }
    }
}
