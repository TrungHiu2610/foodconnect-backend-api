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
