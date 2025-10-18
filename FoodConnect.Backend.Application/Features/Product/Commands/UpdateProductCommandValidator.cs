using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator() 
        {
            RuleFor(x => x.Name)
                    .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");
            
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.Unit)
                .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

            RuleFor(x => x.Status)
                .Must(status => Enum.TryParse(typeof(Domain.Enums.ProductStatusEnum), status, true, out _))
                .WithMessage("Status must be a valid ProductStatusEnum value.");

            When(x => x.NewProductAssets != null && x.NewProductAssets.Any(), () =>
            {
                RuleForEach(x => x.NewProductAssets).SetValidator(new CreateProductAssetDtoValidator());
            });

            When(x => x.UpdateProductAssets != null && x.UpdateProductAssets.Any(), () =>
            {
                RuleForEach(x => x.UpdateProductAssets).SetValidator(new UpdateProductAssetDtoValidator());
            });
        }
    }
}
