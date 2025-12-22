using FluentValidation;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Category.Commands
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator() 
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category Id is required.");
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Category Name must not exceed 100 characters.");
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Category Description must not exceed 500 characters.");
            RuleFor(x=>x.DeliveryType)
                .Must(deliveryType => deliveryType == null || Enum.TryParse(typeof(Domain.Enums.DeliveryTypeEnum), deliveryType, true, out _))
                .WithMessage("DeliveryType must be a valid DeliveryTypeEnum value.");
            RuleFor(x => x.File)
                .Must(file => file == null || file.Length > 0).WithMessage("File must not be empty if provided.");
        }
    }
}
