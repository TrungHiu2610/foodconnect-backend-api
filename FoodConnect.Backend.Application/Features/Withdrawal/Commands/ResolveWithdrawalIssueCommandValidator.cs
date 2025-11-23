using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ResolveWithdrawalIssueCommandValidator : AbstractValidator<ResolveWithdrawalIssueCommand>
{
    private static readonly string[] AllowedResolutions = { "Resolved", "NeedsReinvestigation" };
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
    private const long MaxImageSize = 10 * 1024 * 1024; // 10MB

    public ResolveWithdrawalIssueCommandValidator()
    {
        RuleFor(x => x.WithdrawalRequestId)
            .NotEmpty().WithMessage("WithdrawalRequestId is required");

        RuleFor(x => x.Resolution)
            .NotEmpty().WithMessage("Resolution is required")
            .Must(r => AllowedResolutions.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Resolution must be either 'Resolved' or 'NeedsReinvestigation'");

        RuleFor(x => x.AdminNote)
            .NotEmpty().WithMessage("AdminNote is required")
            .MaximumLength(1000).WithMessage("AdminNote must not exceed 1000 characters");

        RuleFor(x => x.NewProofImage)
            .Must(file => file == null || file.Length <= MaxImageSize)
            .WithMessage("Proof image must not exceed 10MB")
            .Must(file => file == null || AllowedImageExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
            .WithMessage("Proof image must be .jpg, .jpeg, .png, or .pdf");
    }
}
