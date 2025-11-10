using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class ConfirmDeliveryWithProofCommandValidator : AbstractValidator<ConfirmDeliveryWithProofCommand>
    {
        public ConfirmDeliveryWithProofCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order ID is required");

            RuleFor(x => x.DeliveryProofImage)
                .NotNull().WithMessage("Delivery proof image is required");

            When(x => x.DeliveryProofImage != null, () =>
            {
                RuleFor(x => x.DeliveryProofImage.Length)
                    .LessThanOrEqualTo(10 * 1024 * 1024)
                    .WithMessage("Delivery proof image must not exceed 10MB");

                RuleFor(x => x.DeliveryProofImage.ContentType)
                    .Must(contentType => contentType.StartsWith("image/"))
                    .WithMessage("File must be an image");
            });
        }
    }
}
