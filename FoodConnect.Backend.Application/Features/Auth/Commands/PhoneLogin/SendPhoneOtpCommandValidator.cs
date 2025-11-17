using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.PhoneLogin
{
    public class SendPhoneOtpCommandValidator : AbstractValidator<SendPhoneOtpCommand>
    {
        public SendPhoneOtpCommandValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+[1-9]\d{1,14}$").WithMessage("Phone number must be in E.164 format (e.g., +84912345678, +16086866414)");
        }
    }
}
