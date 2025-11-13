using FluentValidation;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class CreateProductReviewCommandValidator : AbstractValidator<CreateProductReviewCommand>
    {
        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        private static readonly string[] AllowedVideoTypes = { "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo" };
        private const int MaxFileSizeMB = 50; // 50MB for videos
        private const int MaxImageSizeMB = 10;
        private const int MaxAssets = 5;

        public CreateProductReviewCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order ID is required");

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5 stars");

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Comment));

            // Validate review assets (images/videos)
            When(x => x.ReviewAssets != null && x.ReviewAssets.Any(), () =>
            {
                RuleFor(x => x.ReviewAssets)
                    .Must(assets => assets == null || assets.Count <= MaxAssets)
                    .WithMessage($"You can upload a maximum of {MaxAssets} images/videos");

                RuleForEach(x => x.ReviewAssets).ChildRules(asset =>
                {
                    asset.RuleFor(file => file.ContentType)
                        .Must(contentType => 
                            AllowedImageTypes.Contains(contentType) || 
                            AllowedVideoTypes.Contains(contentType))
                        .WithMessage("File must be an image (JPEG, PNG, GIF, WebP) or video (MP4, MPEG, MOV, AVI)");

                    asset.RuleFor(file => file.Length)
                        .Must((file, length) =>
                        {
                            if (AllowedImageTypes.Contains(file.ContentType))
                                return length <= MaxImageSizeMB * 1024 * 1024;
                            else if (AllowedVideoTypes.Contains(file.ContentType))
                                return length <= MaxFileSizeMB * 1024 * 1024;
                            return false;
                        })
                        .WithMessage(file => 
                            AllowedImageTypes.Contains(file.ContentType) 
                                ? $"Image must not exceed {MaxImageSizeMB}MB" 
                                : $"Video must not exceed {MaxFileSizeMB}MB");
                });
            });
        }
    }
}
