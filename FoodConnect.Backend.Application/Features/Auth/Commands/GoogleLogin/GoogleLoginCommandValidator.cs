using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.GoogleLogin
{
    public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
    {
        public GoogleLoginCommandValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("Google ID token is required");
        }
    }
}
