using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ProcessWithdrawalRequestCommandValidator : AbstractValidator<ProcessWithdrawalRequestCommand>
{
    public ProcessWithdrawalRequestCommandValidator()
    {
        RuleFor(x => x.WithdrawalRequestId)
            .NotEmpty()
            .WithMessage("Withdrawal request ID is required");

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => !x.IsApproved)
            .WithMessage("Rejection reason is required when rejecting a request")
            .MaximumLength(500)
            .WithMessage("Rejection reason must not exceed 500 characters");

        RuleFor(x => x.AdminNote)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.AdminNote))
            .WithMessage("Admin note must not exceed 500 characters");
    }
}
