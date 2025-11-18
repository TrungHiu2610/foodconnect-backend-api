using FluentValidation;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class UpdatePromotionCommandValidator : AbstractValidator<UpdatePromotionCommand>
    {
        public UpdatePromotionCommandValidator()
        {
            RuleFor(x => x.PromotionId)
                .NotEmpty().WithMessage("Promotion ID is required");

            RuleFor(x => x.PromotionName)
                .MaximumLength(200).WithMessage("Promotion name must not exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.PromotionName));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DiscountValue)
                .GreaterThan(0).WithMessage("Discount value must be greater than 0")
                .When(x => x.DiscountValue.HasValue && x.PromotionType == (int)PromotionTypeEnum.FixedAmount);

            RuleFor(x => x.DiscountValue)
                .GreaterThan(PromotionConstants.MinPercentageDiscount)
                .LessThanOrEqualTo(PromotionConstants.MaxPercentageDiscount)
                .WithMessage($"Percentage discount must be between {PromotionConstants.MinPercentageDiscount} and {PromotionConstants.MaxPercentageDiscount}")
                .When(x => x.DiscountValue.HasValue && x.PromotionType == (int)PromotionTypeEnum.Percentage);

            RuleFor(x => x.MinimumOrderValue)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum order value must be greater than or equal to 0")
                .When(x => x.MinimumOrderValue.HasValue);

            RuleFor(x => x.MaxUsageCount)
                .GreaterThan(0).WithMessage("Max usage count must be greater than 0")
                .When(x => x.MaxUsageCount.HasValue);

            RuleFor(x => x.UsagePerCustomer)
                .GreaterThan(0).WithMessage("Usage per customer must be greater than 0")
                .When(x => x.UsagePerCustomer.HasValue);

            RuleFor(x => x.StartDate)
                .Must(BeAtLeastMinLeadDays).WithMessage($"Start date must be at least {PromotionConstants.MinLeadDays} days from now")
                .When(x => x.StartDate.HasValue);

            RuleFor(x => x)
                .Must(x => !x.EndDate.HasValue || !x.StartDate.HasValue || x.EndDate.Value > x.StartDate.Value)
                .WithMessage("End date must be after start date")
                .When(x => x.EndDate.HasValue && x.StartDate.HasValue);

            RuleFor(x => x.CoverImage)
                .Must(file => file == null || file.Length <= 10 * 1024 * 1024)
                .WithMessage("Cover image size must not exceed 10MB");
        }

        private bool BeAtLeastMinLeadDays(DateTime? startDate)
        {
            if (!startDate.HasValue) return true;
            return startDate.Value >= DateTime.UtcNow.AddDays(PromotionConstants.MinLeadDays);
        }
    }
}
