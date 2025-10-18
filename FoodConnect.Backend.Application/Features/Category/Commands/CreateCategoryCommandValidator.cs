using FluentValidation;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Category.Commands
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
            When(x => x.File != null, () =>
            {
                RuleFor(x => x.File.Length)
                    .LessThanOrEqualTo(100 * 1024 * 1024) // 100MB
                    .WithMessage("File size must be less than or equal to 100MB.");

                RuleFor(x => x.File.ContentType.ToLower())
                    .Must(ct => ct.StartsWith("image"))
                    .WithMessage("File must be image.");
            });
            RuleFor(x => x.DeliveryType)
                .Must(dt => dt == null || Enum.TryParse(typeof(DeliveryTypeEnum), dt, true, out _))
                .WithMessage("DeliveryType must be a valid value from DeliveryTypeEnum");
        }
    }
}
