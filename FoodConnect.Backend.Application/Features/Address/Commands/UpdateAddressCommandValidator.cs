using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Address.Commands
{
    public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
    {
        public UpdateAddressCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Address ID is required");

            RuleFor(x => x.RecipientName)
                .NotEmpty().WithMessage("Recipient name is required")
                .MaximumLength(200).WithMessage("Recipient name must not exceed 200 characters")
                .When(x => x.RecipientName != null);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid phone number format")
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street address is required")
                .MaximumLength(500).WithMessage("Street address must not exceed 500 characters")
                .When(x => x.Street != null);

            RuleFor(x => x.Ward)
                .NotEmpty().WithMessage("Ward is required")
                .MaximumLength(200).WithMessage("Ward must not exceed 200 characters")
                .When(x => x.Ward != null);

            RuleFor(x => x.District)
                .NotEmpty().WithMessage("District is required")
                .MaximumLength(200).WithMessage("District must not exceed 200 characters")
                .When(x => x.District != null);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(200).WithMessage("City must not exceed 200 characters")
                .When(x => x.City != null);

            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("Note must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Note));

            RuleFor(x => x.AddressType)
                .Must(x => x == (int)Domain.Enums.AddressTypeEnum.Home ||
                            x == (int)Domain.Enums.AddressTypeEnum.Work ||
                            x == (int)Domain.Enums.AddressTypeEnum.Other)
                .WithMessage("Address type must be 1 (Home), 2 (Work), or 3 (Other)");
        }
    }
}
