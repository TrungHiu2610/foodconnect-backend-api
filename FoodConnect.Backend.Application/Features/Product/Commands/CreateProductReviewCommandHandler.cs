using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class CreateProductReviewCommandHandler : IRequestHandler<CreateProductReviewCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IBaseRepository<ProductReview> _reviewRepository;
        private readonly IBaseRepository<ProductReviewAsset> _reviewAssetRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;

        public CreateProductReviewCommandHandler(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IBaseRepository<ProductReview> reviewRepository,
            IBaseRepository<ProductReviewAsset> reviewAssetRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IFileStorageService fileStorageService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _reviewRepository = reviewRepository;
            _reviewAssetRepository = reviewAssetRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(CreateProductReviewCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();
            var uploadedAssetUrls = new List<string>();

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Check authorization
                var userId = _currentUserService.UserId;
                if (!userId.HasValue)
                {
                    return result.BuildUnauthorized();
                }

                // Get order with items
                var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
                if (order == null)
                {
                    return result.BuildNotFound("Order not found");
                }

                // Verify buyer owns this order
                if (order.BuyerId != userId.Value)
                {
                    return result.BuildForbidden("You can only review products from your own orders");
                }

                // Validate order is completed
                if (order.Status != OrderStatusEnum.Completed)
                {
                    return result.BuildFail("You can only review products after the order is completed");
                }

                // Verify product is in this order
                var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == request.ProductId);
                if (orderItem == null)
                {
                    return result.BuildFail("Product not found in this order");
                }

                // Check if already reviewed
                var existingReview = await _reviewRepository.GetAsync(
                    r => r.OrderId == request.OrderId && r.ProductId == request.ProductId && r.BuyerId == userId.Value
                );
                if (existingReview != null)
                {
                    return result.BuildConflict("You have already reviewed this product");
                }

                // Create review
                var review = new ProductReview
                {
                    OrderId = request.OrderId,
                    ProductId = request.ProductId,
                    BuyerId = userId.Value,
                    Rating = request.Rating,
                    Comment = request.Comment
                };

                await _reviewRepository.AddAsync(review);
                await _unitOfWork.SaveChangesAsync(); // Save to get review.Id

                // Upload review assets (images/videos) if provided
                if (request.ReviewAssets != null && request.ReviewAssets.Any())
                {
                    for (int i = 0; i < request.ReviewAssets.Count; i++)
                    {
                        var file = request.ReviewAssets[i];
                        
                        // Determine asset type and S3 directory
                        var assetType = file.ContentType.StartsWith("image/") 
                            ? ProductAssetTypeEnum.Image 
                            : ProductAssetTypeEnum.Video;
                        
                        var prefix = assetType == ProductAssetTypeEnum.Image 
                            ? AWSDirectoryConstant.IMAGE_REVIEW 
                            : AWSDirectoryConstant.VIDEO_REVIEW;

                        // Upload file to S3
                        var assetUrl = await _fileStorageService.UploadFileAsync(
                            file,
                            $"{prefix}/{request.ProductId}"
                        );
                        uploadedAssetUrls.Add(assetUrl);

                        // Create ProductReviewAsset record
                        var reviewAsset = new ProductReviewAsset
                        {
                            ProductReviewId = review.Id,
                            AssetUrl = assetUrl,
                            AssetType = assetType,
                            DisplayOrder = i // 0-based index for ordering
                        };

                        await _reviewAssetRepository.AddAsync(reviewAsset);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync(transaction);

                return result.BuildSuccess(
                    new CreateOrUpdateResponse { Id = review.Id },
                    "Product review submitted successfully"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Delete all uploaded assets if transaction fails
                foreach (var assetUrl in uploadedAssetUrls)
                {
                    try
                    {
                        await _fileStorageService.DeleteFileAsync(assetUrl);
                    }
                    catch
                    {
                        // Log but don't fail if cleanup fails
                    }
                }

                return result.BuildFail($"Failed to submit review: {ex.Message}");
            }
        }
    }
}
