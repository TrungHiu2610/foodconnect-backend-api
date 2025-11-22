using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Payment.Commands;

public class CreateVNPayPaymentCommandValidator : AbstractValidator<CreateVNPayPaymentCommand>
{
    public CreateVNPayPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");
    }
}
