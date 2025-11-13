using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class SellerRespondToReviewCommandHandler : IRequestHandler<SellerRespondToReviewCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IBaseRepository<ProductReview> _reviewRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public SellerRespondToReviewCommandHandler(
            IBaseRepository<ProductReview> reviewRepository,
            IShopRepository shopRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _reviewRepository = reviewRepository;
            _shopRepository = shopRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(SellerRespondToReviewCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            try
            {
                // Check authorization
                var userId = _currentUserService.UserId;
                if (!userId.HasValue)
                {
                    return result.BuildUnauthorized();
                }

                // Get seller's shop
                var shop = await _shopRepository.GetByUserIdAsync(userId.Value);
                if (shop == null)
                {
                    return result.BuildForbidden("You must have a shop to respond to reviews");
                }

                // Get review with product details
                var review = await _reviewRepository.GetAsync(
                    r => r.Id == request.ReviewId,
                    r => r.Product
                );

                if (review == null)
                {
                    return result.BuildNotFound("Review not found");
                }

                // Verify the review is for a product in seller's shop
                if (review.Product.ShopId != shop.Id)
                {
                    return result.BuildForbidden("You can only respond to reviews for your own products");
                }

                // Check if already responded
                if (!string.IsNullOrEmpty(review.SellerResponse))
                {
                    return result.BuildConflict("You have already responded to this review");
                }

                // Update review with seller response
                review.SellerResponse = request.SellerResponse;
                review.SellerRespondedAt = DateTime.UtcNow;

                _reviewRepository.Update(review);
                await _unitOfWork.SaveChangesAsync();

                return result.BuildSuccess(
                    new CreateOrUpdateResponse { Id = review.Id },
                    "Response submitted successfully"
                );
            }
            catch (Exception ex)
            {
                return result.BuildFail($"Failed to submit response: {ex.Message}");
            }
        }
    }
}
