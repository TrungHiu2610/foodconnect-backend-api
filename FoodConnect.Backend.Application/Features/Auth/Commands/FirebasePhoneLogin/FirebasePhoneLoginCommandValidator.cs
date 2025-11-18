using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.FirebasePhoneLogin
{
    public class FirebasePhoneLoginCommandValidator : AbstractValidator<FirebasePhoneLoginCommand>
    {
        public FirebasePhoneLoginCommandValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("Firebase ID token is required")
                .MinimumLength(10).WithMessage("Invalid Firebase ID token format");

            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.FullName));
        }
    }
}
