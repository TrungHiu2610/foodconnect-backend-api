using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class UpdatePromotionCommandHandler : IRequestHandler<UpdatePromotionCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPromotionProductRepository _promotionProductRepository;
        private readonly IProductRepository _productRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdatePromotionCommandHandler(
            IPromotionRepository promotionRepository,
            IPromotionProductRepository promotionProductRepository,
            IProductRepository productRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _promotionRepository = promotionRepository;
            _promotionProductRepository = promotionProductRepository;
            _productRepository = productRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return result.BuildUnauthorized();
            }

            var promotion = await _promotionRepository.GetDetailByIdAsync(request.PromotionId);
            if (promotion == null)
            {
                return result.BuildNotFound("Promotion not found");
            }

            if (promotion.Shop.UserId != userId.Value)
            {
                return result.BuildForbidden();
            }

            if (promotion.Status != PromotionStatusEnum.Draft && promotion.Status != PromotionStatusEnum.PendingApproval)
            {
                return result.BuildFail("Can only update promotions in Draft or PendingApproval status");
            }

            if (request.ApplicableToAllProducts == false && (request.ProductIds == null || !request.ProductIds.Any()))
            {
                return result.BuildFail("Product list is required when not applicable to all products");
            }

            if (request.ApplicableToAllProducts == false && request.ProductIds != null)
            {
                var products = await _productRepository.GetByIdsAsync(request.ProductIds);
                var notOwnedProducts = products.Where(p => p.ShopId != promotion.ShopId).ToList();
                if (notOwnedProducts.Any())
                {
                    return result.BuildFail("You can only add your own products to the promotion");
                }
            }

            string? oldImageUrl = promotion.CoverImageUrl;
            string? newImageUrl = null;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (request.CoverImage != null)
                {
                    newImageUrl = await _fileStorageService.UploadFileAsync(
                        request.CoverImage,
                        $"Images/Promotions/{promotion.ShopId}");
                    promotion.CoverImageUrl = newImageUrl;
                }

                if (!string.IsNullOrEmpty(request.PromotionName))
                    promotion.PromotionName = request.PromotionName;

                if (request.Description != null)
                    promotion.Description = request.Description;

                if (request.PromotionType.HasValue)
                    promotion.PromotionType = (PromotionTypeEnum)request.PromotionType.Value;

                if (request.DiscountValue.HasValue)
                    promotion.DiscountValue = request.DiscountValue.Value;

                if (request.MinimumOrderValue.HasValue)
                    promotion.MinimumOrderValue = request.MinimumOrderValue.Value;

                if (request.MaxUsageCount.HasValue)
                    promotion.MaxUsageCount = request.MaxUsageCount.Value;

                if (request.UsagePerCustomer.HasValue)
                    promotion.UsagePerCustomer = request.UsagePerCustomer.Value;

                if (request.StartDate.HasValue)
                    promotion.StartDate = request.StartDate.Value.ToUniversalTime();

                if (request.EndDate.HasValue)
                    promotion.EndDate = request.EndDate.Value.ToUniversalTime();

                if (request.ApplicableToAllProducts.HasValue)
                    promotion.ApplicableToAllProducts = request.ApplicableToAllProducts.Value;

                _promotionRepository.Update(promotion);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (request.ProductIds != null)
                {
                    await _promotionProductRepository.DeleteByPromotionIdAsync(request.PromotionId);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    if (!promotion.ApplicableToAllProducts)
                    {
                        var promotionProducts = request.ProductIds.Select(productId => new Domain.Entities.PromotionProduct
                        {
                            PromotionId = promotion.Id,
                            ProductId = productId
                        }).ToList();

                        await _promotionProductRepository.AddRangeAsync(promotionProducts);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }

                await _unitOfWork.CommitTransactionAsync(transaction);

                if (!string.IsNullOrEmpty(oldImageUrl) && !string.IsNullOrEmpty(newImageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(oldImageUrl);
                }

                return result.BuildSuccess(new CreateOrUpdateResponse { Id = promotion.Id }, "Promotion updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (!string.IsNullOrEmpty(newImageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(newImageUrl);
                }

                return result.BuildFail($"Failed to update promotion: {ex.Message}");
            }
        }
    }
}
