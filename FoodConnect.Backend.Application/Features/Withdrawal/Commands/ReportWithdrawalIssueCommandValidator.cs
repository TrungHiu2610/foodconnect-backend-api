using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ReportWithdrawalIssueCommandValidator : AbstractValidator<ReportWithdrawalIssueCommand>
{
    public ReportWithdrawalIssueCommandValidator()
    {
        RuleFor(x => x.WithdrawalRequestId)
            .NotEmpty()
            .WithMessage("Withdrawal request ID is required");

        RuleFor(x => x.IssueDescription)
            .NotEmpty()
            .WithMessage("Issue description is required")
            .MaximumLength(1000)
            .WithMessage("Issue description must not exceed 1000 characters");
    }
}
