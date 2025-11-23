using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Complaint.Commands.RejectComplaint;

public class RejectComplaintCommandValidator : AbstractValidator<RejectComplaintCommand>
{
    public RejectComplaintCommandValidator()
    {
        RuleFor(x => x.ComplaintId)
            .NotEmpty().WithMessage("Complaint ID is required");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MaximumLength(1000).WithMessage("Rejection reason must not exceed 1000 characters");
    }
}
