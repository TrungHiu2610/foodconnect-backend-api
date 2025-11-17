using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.EmailRegister
{
    public class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
    {
        public VerifyEmailOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required")
                .Length(6).WithMessage("OTP must be 6 digits")
                .Matches(@"^\d{6}$").WithMessage("OTP must be numeric");
        }
    }
}
