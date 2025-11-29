using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Complaint.Commands.ApproveComplaint;

public class ApproveComplaintCommandValidator : AbstractValidator<ApproveComplaintCommand>
{
    public ApproveComplaintCommandValidator()
    {
        RuleFor(x => x.ComplaintId)
            .NotEmpty().WithMessage("Complaint ID is required");

        RuleFor(x => x.RefundAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Refund amount must be greater than or equal to 0");

        RuleFor(x => x.AdminReason)
            .MaximumLength(1000).WithMessage("Admin reason must not exceed 1000 characters");
    }
}
