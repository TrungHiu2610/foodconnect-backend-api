using FluentValidation;
using FoodConnect.Backend.Application.Commons.Constants;

namespace FoodConnect.Backend.Application.Features.Cart.Commands
{
    public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
    {
        public UpdateCartItemCommandValidator()
        {
            RuleFor(x => x.CartItemId)
                .NotEmpty().WithMessage("Cart item ID is required.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0.")
                .LessThanOrEqualTo(CartConstants.MaxQuantityPerProduct)
                .WithMessage($"Quantity must not exceed {CartConstants.MaxQuantityPerProduct}.");
        }
    }
}
