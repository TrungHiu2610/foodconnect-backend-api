using FluentValidation;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class CreateWithdrawalRequestCommandValidator : AbstractValidator<CreateWithdrawalRequestCommand>
{
    public CreateWithdrawalRequestCommandValidator()
    {
        RuleFor(x => x.RequestedAmount)
            .GreaterThan(0)
            .WithMessage("Requested amount must be greater than 0");

        RuleFor(x => x.PaymentMethod)
            .Must(value => Enum.IsDefined(typeof(PaymentMethodEnum), value))
            .WithMessage("Invalid payment method");

        RuleFor(x => x.PaymentAccountNumber)
            .NotEmpty()
            .WithMessage("Payment account number is required")
            .MaximumLength(50)
            .WithMessage("Payment account number must not exceed 50 characters");

        RuleFor(x => x.PaymentAccountName)
            .NotEmpty()
            .WithMessage("Payment account name is required")
            .MaximumLength(200)
            .WithMessage("Payment account name must not exceed 200 characters");

        RuleFor(x => x.SellerNote)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.SellerNote))
            .WithMessage("Seller note must not exceed 500 characters");
    }
}
