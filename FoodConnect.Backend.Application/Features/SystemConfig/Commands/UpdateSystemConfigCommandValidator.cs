using FluentValidation;

namespace FoodConnect.Backend.Application.Features.SystemConfig.Commands;

public class UpdateSystemConfigCommandValidator : AbstractValidator<UpdateSystemConfigCommand>
{
    public UpdateSystemConfigCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Config ID is required");

        RuleFor(x => x.ConfigKey)
            .NotEmpty()
            .WithMessage("Config key is required")
            .MaximumLength(100)
            .WithMessage("Config key must not exceed 100 characters")
            .Matches("^[A-Za-z0-9_]+$")
            .WithMessage("Config key can only contain letters, numbers, and underscores");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.DataType)
            .NotEmpty()
            .WithMessage("Data type is required")
            .Must(x => new[] { "String", "Number", "Decimal", "Boolean", "Text" }.Contains(x))
            .WithMessage("Data type must be one of: String, Number, Decimal, Boolean, Text");

        RuleFor(x => x.Type)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Type must be a valid enum value");
    }
}
