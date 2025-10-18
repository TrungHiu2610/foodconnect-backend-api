using FluentValidation;
using FoodConnect.Backend.Application.Commons.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class UpdateProductAssetDtoValidator : AbstractValidator<ProductAssetUpdateDto>
    {
        public UpdateProductAssetDtoValidator() 
        {
            RuleFor(x => x.AssetDescription)
                .MaximumLength(500).WithMessage("Asset description must not exceed 500 characters.");
        }
    }
}
