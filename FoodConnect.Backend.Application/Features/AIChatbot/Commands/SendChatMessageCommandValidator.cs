using FluentValidation;

namespace FoodConnect.Backend.Application.Features.AIChatbot.Commands;

public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    public SendChatMessageCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(500).WithMessage("Message must not exceed 500 characters");
    }
}
