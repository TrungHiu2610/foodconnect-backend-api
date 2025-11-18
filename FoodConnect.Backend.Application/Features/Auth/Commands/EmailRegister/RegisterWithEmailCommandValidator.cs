using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.EmailRegister
{
    public class RegisterWithEmailCommandValidator : AbstractValidator<RegisterWithEmailCommand>
    {
        public RegisterWithEmailCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters");
        }
    }
}
