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

            RuleFor(x => x.OwnerName)
                .NotEmpty().WithMessage("Owner name is required")
                .MaximumLength(100).WithMessage("Owner name must not exceed 100 characters");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Phone number is invalid");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Email is invalid");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

            RuleFor(x => x.PayoutMethod)
                .NotEmpty().WithMessage("Payout method is required")
                .Must(x => x == "Bank" || x == "MoMo")
                .WithMessage("Payout method must be 'Bank' or 'MoMo'");

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
        }
    }
}
