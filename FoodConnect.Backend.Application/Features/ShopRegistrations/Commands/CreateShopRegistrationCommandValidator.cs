using FluentValidation;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Commands
{
    public class CreateShopRegistrationCommandValidator : AbstractValidator<CreateShopRegistrationCommand>
    {
        public CreateShopRegistrationCommandValidator() 
        {
            RuleFor(x => x.ShopName)
                .NotEmpty().WithMessage("Shop name is required.")
                .MaximumLength(255).WithMessage("Shop name must not exceed 255 characters.");
            
            RuleFor(x => x.ShopDescription)
                .NotEmpty().WithMessage("Shop description is required.");
            
            RuleFor(x => x.PayoutMethod)
                .InclusiveBetween((short)1, (short)2).WithMessage("Payout method is invalid.");
            
            RuleFor(x => x.PayoutAccountInfo)
                .NotEmpty().WithMessage("Payout account info is required.")
                .MaximumLength(20).WithMessage("Payout account info must not exceed 20 characters.");
            
            RuleFor(x => x.PayoutAccountName)
                .NotEmpty().WithMessage("Payout account name is required.")
                .MaximumLength(100).WithMessage("Payout account name must not exceed 100 characters.");
            
            // Address validations
            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street is required.")
                .MaximumLength(255).WithMessage("Street must not exceed 255 characters.");
            
            RuleFor(x => x.Ward)
                .NotEmpty().WithMessage("Ward is required.")
                .MaximumLength(100).WithMessage("Ward must not exceed 100 characters.");
            
            RuleFor(x => x.District)
                .NotEmpty().WithMessage("District is required.")
                .MaximumLength(100).WithMessage("District must not exceed 100 characters.");
            
            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(100).WithMessage("City must not exceed 100 characters.");
            
            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required.")
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");
            
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");
            
            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
            
            // Categories validation
            RuleFor(x => x.CategoryIds)
                .NotEmpty().WithMessage("At least one category is required.")
                .Must(categories => categories != null && categories.Count > 0)
                .WithMessage("At least one category is required.");
            
            // Operating Hours validation
            RuleFor(x => x.OperatingHours)
                .NotEmpty().WithMessage("At least one operating hour is required.")
                .Must(hours => hours != null && hours.Count > 0)
                .WithMessage("At least one operating hour is required.");
            
            RuleForEach(x => x.OperatingHours)
                .ChildRules(hours => {
                    hours.RuleFor(h => h.OpenTime)
                        .LessThan(h => h.CloseTime)
                        .WithMessage("Open time must be before close time.");
                });
            
            // File validations
            RuleFor(x => x.IdCardFront)
                .NotNull().WithMessage("ID card front image is required.");
            
            RuleFor(x => x.IdCardBack)
                .NotNull().WithMessage("ID card back image is required.");
            
            RuleFor(x => x.PortraitPhoto)
                .NotNull().WithMessage("Portrait photo is required.");
            
            RuleFor(x => x.KitchenPhotos)
                .NotNull().WithMessage("At least one kitchen photo is required.")
                .Must(kitchenPhotos => kitchenPhotos != null && kitchenPhotos.Count > 0)
                .WithMessage("At least one kitchen photo is required.");
        }
    }
}
