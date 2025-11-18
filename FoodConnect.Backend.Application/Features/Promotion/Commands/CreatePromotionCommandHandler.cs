using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Features.Promotion.Services;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class CreatePromotionCommandHandler : IRequestHandler<CreatePromotionCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPromotionProductRepository _promotionProductRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IProductRepository _productRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly PromotionNotificationService _promotionNotificationService;

        public CreatePromotionCommandHandler(
            IPromotionRepository promotionRepository,
            IPromotionProductRepository promotionProductRepository,
            IShopRepository shopRepository,
            IProductRepository productRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            PromotionNotificationService promotionNotificationService)
        {
            _promotionRepository = promotionRepository;
            _promotionProductRepository = promotionProductRepository;
            _shopRepository = shopRepository;
            _productRepository = productRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _promotionNotificationService = promotionNotificationService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return result.BuildUnauthorized();
            }

            var shop = await _shopRepository.GetByUserIdAsync(userId.Value);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            if (shop.Status != ShopStatusEnum.Active)
            {
                return result.BuildFail("Shop must be active to create promotions");
            }

            if (!request.ApplicableToAllProducts && (request.ProductIds == null || !request.ProductIds.Any()))
            {
                return result.BuildFail("Product list is required when not applicable to all products");
            }

            if (!request.ApplicableToAllProducts)
            {
                var products = await _productRepository.GetByIdsAsync(request.ProductIds!);
                var invalidProducts = request.ProductIds!.Except(products.Select(p => p.Id)).ToList();

                if (invalidProducts.Any())
                {
                    return result.BuildFail("Some products are invalid");
                }

                var notOwnedProducts = products.Where(p => p.ShopId != shop.Id).ToList();
                if (notOwnedProducts.Any())
                {
                    return result.BuildFail("You can only create promotions for your own products");
                }
            }

            string? coverImageUrl = null;
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (request.CoverImage != null)
                {
                    coverImageUrl = await _fileStorageService.UploadFileAsync(
                        request.CoverImage,
                        $"Images/Promotions/{shop.Id}");
                }

                var promotion = new Domain.Entities.Promotion
                {
                    ShopId = shop.Id,
                    PromotionName = request.PromotionName,
                    Description = request.Description,
                    CoverImageUrl = coverImageUrl,
                    PromotionType = (PromotionTypeEnum)request.PromotionType,
                    DiscountValue = request.DiscountValue,
                    MinimumOrderValue = request.MinimumOrderValue,
                    MaxUsageCount = request.MaxUsageCount,
                    UsagePerCustomer = request.UsagePerCustomer,
                    StartDate = request.StartDate.ToUniversalTime(),
                    EndDate = request.EndDate.ToUniversalTime(),
                    Status = PromotionStatusEnum.Draft,
                    ApplicableToAllProducts = request.ApplicableToAllProducts
                };

                await _promotionRepository.AddAsync(promotion);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (!request.ApplicableToAllProducts && request.ProductIds != null)
                {
                    var promotionProducts = request.ProductIds.Select(productId => new Domain.Entities.PromotionProduct
                    {
                        PromotionId = promotion.Id,
                        ProductId = productId
                    }).ToList();

                    await _promotionProductRepository.AddRangeAsync(promotionProducts);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                await _unitOfWork.CommitTransactionAsync(transaction);

                // Send notification to shop owner
                var fullPromotion = await _promotionRepository.GetDetailByIdAsync(promotion.Id);
                if (fullPromotion != null)
                {
                    await _promotionNotificationService.NotifyPromotionCreatedAsync(fullPromotion, cancellationToken);
                }

                return result.BuildSuccess(new CreateOrUpdateResponse { Id = promotion.Id }, "Promotion created successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (!string.IsNullOrEmpty(coverImageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(coverImageUrl);
                }

                return result.BuildFail($"Failed to create promotion: {ex.Message}");
            }
        }
    }
}
