using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.PhoneLogin
{
    public class VerifyPhoneOtpCommandValidator : AbstractValidator<VerifyPhoneOtpCommand>
    {
        public VerifyPhoneOtpCommandValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required")
                .Length(6).WithMessage("OTP must be 6 digits")
                .Matches(@"^\d{6}$").WithMessage("OTP must be numeric");
        }
    }
}
