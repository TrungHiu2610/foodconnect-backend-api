using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class UpdateShopCommandValidator : AbstractValidator<UpdateShopCommand>
    {
        public UpdateShopCommandValidator()
        {
            RuleFor(x => x.ShopId)
                .NotEmpty().WithMessage("Shop ID is required");

            // Only validate if provided (nullable for partial update)
            When(x => x.ShopName != null, () =>
            {
                RuleFor(x => x.ShopName!)
                    .NotEmpty().WithMessage("Shop name cannot be empty")
                    .MaximumLength(200).WithMessage("Shop name must not exceed 200 characters");
            });

            When(x => x.PayoutMethod != null, () =>
            {
                RuleFor(x => x.PayoutMethod!.Value)
                    .Must(x => x == 1 || x == 2)
                    .WithMessage("Payout method must be 1 (MoMo) or 2 (VNPay)");
            });

            When(x => x.PayoutAccountInfo != null, () =>
            {
                RuleFor(x => x.PayoutAccountInfo!)
                    .NotEmpty().WithMessage("Payout account info cannot be empty")
                    .MaximumLength(50).WithMessage("Payout account info must not exceed 50 characters");
            });

            When(x => x.PayoutAccountName != null, () =>
            {
                RuleFor(x => x.PayoutAccountName!)
                    .NotEmpty().WithMessage("Payout account name cannot be empty")
                    .MaximumLength(100).WithMessage("Payout account name must not exceed 100 characters");
            });

            When(x => x.CategoryIds != null, () =>
            {
                RuleFor(x => x.CategoryIds!)
                    .Must(x => x.Count > 0)
                    .WithMessage("At least one category is required");
            });

            // File validations (optional for update)
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

            When(x => x.Logo != null, () =>
            {
                RuleFor(x => x.Logo!.Length)
                    .LessThanOrEqualTo(10 * 1024 * 1024)
                    .WithMessage("Logo must not exceed 10MB");
            });

            When(x => x.CoverImage != null, () =>
            {
                RuleFor(x => x.CoverImage!.Length)
                    .LessThanOrEqualTo(10 * 1024 * 1024)
                    .WithMessage("Cover image must not exceed 10MB");
            });

            When(x => x.FoodSafetyCertificate != null, () =>
            {
                RuleFor(x => x.FoodSafetyCertificate!.Length)
                    .LessThanOrEqualTo(10 * 1024 * 1024)
                    .WithMessage("Food safety certificate must not exceed 10MB");
            });

            When(x => x.KitchenPhotos != null && x.KitchenPhotos.Any(), () =>
            {
                RuleFor(x => x.KitchenPhotos!)
                    .Must(photos => photos.All(p => p.Length <= 10 * 1024 * 1024))
                    .WithMessage("Each kitchen photo must not exceed 10MB");
            });
        }
    }
}
