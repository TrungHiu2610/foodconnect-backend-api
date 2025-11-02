using FluentValidation;

namespace FoodConnect.Backend.Application.Features.User.Commands
{
    public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(200).WithMessage("Full name must not exceed 200 characters");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid phone number format")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Gender)
                .Must(g => g == null || new[] { "Male", "Female", "Other" }.Contains(g))
                .WithMessage("Gender must be Male, Female, or Other");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Now).WithMessage("Date of birth must be in the past")
                .GreaterThan(DateTime.Now.AddYears(-150)).WithMessage("Invalid date of birth")
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x.Avatar)
                .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
                .WithMessage("Avatar file size must not exceed 5MB")
                .Must(file => file == null || new[] { ".jpg", ".jpeg", ".png", ".gif" }
                    .Contains(Path.GetExtension(file.FileName).ToLower()))
                .WithMessage("Avatar must be an image file (jpg, jpeg, png, gif)");
        }
    }
}
