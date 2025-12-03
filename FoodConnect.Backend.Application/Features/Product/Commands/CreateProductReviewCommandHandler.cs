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
                var userId = _currentUserService.UserId;
                if (!userId.HasValue)
                {
                    return result.BuildUnauthorized();
                }

                var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
                if (order == null)
                {
                    return result.BuildNotFound("Order not found");
                }

                if (order.BuyerId != userId.Value)
                {
                    return result.BuildForbidden("You can only review products from your own orders");
                }

                if (order.Status != OrderStatusEnum.Completed)
                {
                    return result.BuildFail("You can only review products after the order is completed");
                }

                var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == request.ProductId);
                if (orderItem == null)
                {
                    return result.BuildFail("Product not found in this order");
                }

                var existingReview = await _reviewRepository.GetAsync(
                    r => r.OrderId == request.OrderId && r.ProductId == request.ProductId && r.BuyerId == userId.Value
                );
                if (existingReview != null)
                {
                    return result.BuildConflict("You have already reviewed this product");
                }

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

                if (request.ReviewAssets != null && request.ReviewAssets.Any())
                {
                    for (int i = 0; i < request.ReviewAssets.Count; i++)
                    {
                        var file = request.ReviewAssets[i];
                        
                        var assetType = file.ContentType.StartsWith("image/") 
                            ? ProductAssetTypeEnum.Image 
                            : ProductAssetTypeEnum.Video;
                        
                        var prefix = assetType == ProductAssetTypeEnum.Image 
                            ? AWSDirectoryConstant.IMAGE_REVIEW 
                            : AWSDirectoryConstant.VIDEO_REVIEW;

                        var assetUrl = await _fileStorageService.UploadFileAsync(
                            file,
                            $"{prefix}/{request.ProductId}"
                        );
                        uploadedAssetUrls.Add(assetUrl);

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

                foreach (var assetUrl in uploadedAssetUrls)
                {
                    try
                    {
                        await _fileStorageService.DeleteFileAsync(assetUrl);
                    }
                    catch
                    {
                    }
                }

                return result.BuildFail($"Failed to submit review: {ex.Message}");
            }
        }
    }
}
