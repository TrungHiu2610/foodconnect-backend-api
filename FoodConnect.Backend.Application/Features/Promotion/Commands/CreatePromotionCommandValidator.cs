using FluentValidation;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
    {
        public CreatePromotionCommandValidator()
        {
            RuleFor(x => x.PromotionName)
                .NotEmpty().WithMessage("Promotion name is required")
                .MaximumLength(200).WithMessage("Promotion name must not exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

            RuleFor(x => x.PromotionType)
                .Must(v => Enum.IsDefined(typeof(PromotionTypeEnum), v))
                .WithMessage("Invalid promotion type");

            RuleFor(x => x.DiscountValue)
                .GreaterThan(0).WithMessage("Discount value must be greater than 0")
                .When(x => x.PromotionType == (int)PromotionTypeEnum.FixedAmount);

            RuleFor(x => x.DiscountValue)
                .GreaterThan(PromotionConstants.MinPercentageDiscount)
                .LessThanOrEqualTo(PromotionConstants.MaxPercentageDiscount)
                .WithMessage($"Percentage discount must be between {PromotionConstants.MinPercentageDiscount} and {PromotionConstants.MaxPercentageDiscount}")
                .When(x => x.PromotionType == (int)PromotionTypeEnum.Percentage);

            RuleFor(x => x.MinimumOrderValue)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum order value must be greater than or equal to 0");

            RuleFor(x => x.MaxUsageCount)
                .GreaterThan(0).WithMessage("Max usage count must be greater than 0")
                .When(x => x.MaxUsageCount.HasValue);

            RuleFor(x => x.UsagePerCustomer)
                .GreaterThan(0).WithMessage("Usage per customer must be greater than 0");

            RuleFor(x => x.StartDate)
                .Must(BeAtLeastMinLeadDays).WithMessage($"Start date must be at least {PromotionConstants.MinLeadDays} days from now");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

            RuleFor(x => x.ProductIds)
                .NotEmpty().WithMessage("Product list is required when not applicable to all products")
                .When(x => !x.ApplicableToAllProducts);

            RuleFor(x => x.CoverImage)
                .Must(file => file == null || file.Length <= 10 * 1024 * 1024)
                .WithMessage("Cover image size must not exceed 10MB");
        }

        private bool BeAtLeastMinLeadDays(DateTime startDate)
        {
            return startDate >= DateTime.UtcNow.AddDays(PromotionConstants.MinLeadDays);
        }
    }
}
