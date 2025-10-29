using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class CreateShopCommandValidator : AbstractValidator<CreateShopCommand>
    {
        public CreateShopCommandValidator()
        {
            RuleFor(x => x.ShopName)
                .NotEmpty().WithMessage("Shop name is required")
                .MaximumLength(200).WithMessage("Shop name must not exceed 200 characters");

            // Seller information validation
            RuleFor(x => x.SellerFullName)
                .NotEmpty().WithMessage("Seller full name is required")
                .MaximumLength(100).WithMessage("Seller full name must not exceed 100 characters");

            RuleFor(x => x.SellerEmail)
                .NotEmpty().WithMessage("Seller email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Seller email must not exceed 100 characters");

            RuleFor(x => x.SellerPhone)
                .NotEmpty().WithMessage("Seller phone is required")
                .Matches(@"^(0|\+84)[0-9]{9,10}$").WithMessage("Invalid phone number format")
                .MaximumLength(15).WithMessage("Seller phone must not exceed 15 characters");

            RuleFor(x => x.PayoutMethod)
                .NotEmpty().WithMessage("Payout method is required")
                .Must(x => x == 0 || x == 1)  // Bank = 0, MoMo = 1
                .WithMessage("Payout method must be 0 (Bank) or 1 (MoMo)");

            RuleFor(x => x.PayoutAccountInfo)
                .NotEmpty().WithMessage("Payout account info is required")
                .MaximumLength(50).WithMessage("Payout account info must not exceed 50 characters");

            RuleFor(x => x.PayoutAccountName)
                .NotEmpty().WithMessage("Payout account name is required")
                .MaximumLength(100).WithMessage("Payout account name must not exceed 100 characters");

            RuleFor(x => x.CategoryIds)
                .NotEmpty().WithMessage("At least one category is required")
                .Must(x => x != null && x.Count > 0)
                .WithMessage("At least one category is required");

            // File validations
            RuleFor(x => x.IdCardFront)
                .NotNull().WithMessage("ID card front is required");

            RuleFor(x => x.IdCardBack)
                .NotNull().WithMessage("ID card back is required");

            RuleFor(x => x.PortraitPhoto)
                .NotNull().WithMessage("Portrait photo is required");

            RuleFor(x => x.FoodSafetyCertificate)
                .NotNull().WithMessage("Food safety certificate is required");

            // File size validations
            When(x => x.IdCardFront != null, () =>
            {
                RuleFor(x => x.IdCardFront!.Length)
                    .LessThanOrEqualTo(10 * 1024 * 1024)
                    .WithMessage("ID card front must not exceed 10MB");
            });

            When(x => x.IdCardBack != null, () =>
            {
                RuleFor(x => x.IdCardBack!.Length)
                    .LessThanOrEqualTo(10 * 1024 * 1024)
                    .WithMessage("ID card back must not exceed 10MB");
            });

            When(x => x.PortraitPhoto != null, () =>
            {
                RuleFor(x => x.PortraitPhoto!.Length)
                    .LessThanOrEqualTo(10 * 1024 * 1024)
                    .WithMessage("Portrait photo must not exceed 10MB");
            });

            When(x => x.FoodSafetyCertificate != null, () =>
            {
                RuleFor(x => x.FoodSafetyCertificate!.Length)
                    .LessThanOrEqualTo(10 * 1024 * 1024)
                    .WithMessage("Food safety certificate must not exceed 10MB");
            });
        }
    }
}
