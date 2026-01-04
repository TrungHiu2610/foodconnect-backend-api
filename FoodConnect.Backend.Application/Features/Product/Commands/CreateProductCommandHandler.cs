using AutoMapper;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Features.Wishlist.Services;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, BaseResponse<CreateProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IShopRepository _shopRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ShopFollowerNotificationService _shopFollowerNotificationService;
        private readonly IRedisService _redisService;

        public CreateProductCommandHandler(IProductRepository productRepository, IShopRepository shopRepository,
            ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper, 
            IFileStorageService fileStorageService, ICurrentUserService currentUserService, IRedisService redisService,
            ShopFollowerNotificationService shopFollowerNotificationService)
        {
            _productRepository = productRepository;
            _shopRepository = shopRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _currentUserService = currentUserService;
            _redisService = redisService;
            _shopFollowerNotificationService = shopFollowerNotificationService;
        }
        public async Task<BaseResponse<CreateProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateProductResponse>();
            var response = new CreateProductResponse();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildFail("User not found");
            }

            var shop = await _shopRepository.GetByUserIdAsync((Guid)userId);
            if (shop == null)
            {
                return result.BuildFail("Shop not found for this user");
            }

            if (shop.Status != ShopStatusEnum.Active)
            {
                return result.BuildFail("Shop is not active");
            }

            if(shop.User.Id !=  userId)
            {
                return result.BuildFail("You do not have permission to create product for this shop");
            }

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
            {
                return result.BuildFail("Category not found");
            }

            var hasChildren = await _categoryRepository.HasChildrenAsync(request.CategoryId);
            if (hasChildren)
            {
                return result.BuildFail("Product can only be assigned to a leaf category (category without sub-categories)");
            }

            var shopCategoryIds = await _shopRepository.GetAllCategoryIdsForShopAsync(shop.Id);
            if (!shopCategoryIds.Contains(request.CategoryId))
            {
                return result.BuildFail("Category is not in the list of categories that your shop is registered to sell");
            }

            var product = _mapper.Map<Domain.Entities.Product>(request);
            product.ShopId = shop.Id;
            product.Id = Guid.NewGuid();

            if (category.DeliveryType == DeliveryTypeEnum.Standard)
            {
                product.IsAvailable = request.StockQuantity > 0;
            }
            else
            {
                product.IsAvailable = request.IsAvailable;
                product.StockQuantity = null;
            }

            var uploadedFiles = new List<string>();
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (request.ProductAssets != null)
                {
                    if (request.ProductAssets.All(a => !a.IsThumbnail))
                    {
                        return result.BuildFail("Thumbnail is required");
                    }
                    if (request.ProductAssets.Count(a => a.IsThumbnail) > 1)
                    {
                        return result.BuildFail("Only one thumbnail is allowed");
                    }

                    foreach (var assetDto in request.ProductAssets)
                    {
                        if (assetDto.File != null)
                        {
                            assetDto.AssetName = assetDto.File.FileName;
                            var prefix = (assetDto.File.ContentType.ToLower().StartsWith("image")) ? AWSDirectoryConstant.IMAGE_PRODUCT : AWSDirectoryConstant.VIDEO_PRODUCT;
                            prefix += $"/{shop.Id}/{product.Id}";
                            
                            var url = await _fileStorageService.UploadFileAsync(assetDto.File, prefix);
                            uploadedFiles.Add(url);
                            assetDto.AssetUrl = url;
                        }
                    }

                    product.ProductAssets = _mapper.Map<ICollection<ProductAsset>>(request.ProductAssets);
                }

                await _productRepository.AddAsync(product);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                // Invalidate product list cache after successful product creation
                await InvalidateProductCacheAsync(shop.Id, product.CategoryId);

                response.Id = product.Id;
                response.IsSuccess = true;

                // Notify shop followers about new product (only if product is active and available)
                if (product.Status == ProductStatusEnum.Active && product.IsAvailable)
                {
                    // Load product with full details for notification
                    var fullProduct = await _productRepository.GetByIdAsync(product.Id);
                    if (fullProduct != null)
                    {
                        _ = _shopFollowerNotificationService.NotifyFollowersAboutNewProductAsync(fullProduct, cancellationToken);
                    }
                }

                return result.BuildSuccess(response, "Create product success");
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                if (uploadedFiles.Any())
                {
                    foreach (var fileUrl in uploadedFiles)
                    {
                        try
                        {
                            await _fileStorageService.DeleteFileAsync(fileUrl);
                        }
                        catch (Exception delEx)
                        {
                            return result.BuildFail($"Failed to delete uploaded file: {delEx.Message}");
                        }
                    }
                }
                return result.BuildFail($"Create product failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Invalidates product list cache when a product is created/updated/deleted
        /// Strategy: Invalidate all cached product lists for the affected shop and category
        /// </summary>
        private async Task InvalidateProductCacheAsync(Guid shopId, Guid categoryId)
        {
            // Invalidate all product list caches - using pattern matching
            // This ensures all variations of the query (different filters, pagination, etc.) are cleared
            await _redisService.DeleteByPatternAsync("products:list:*");
            
            // Note: We could be more granular by only invalidating specific patterns like:
            // - products:list:*:{shopId}:*
            // - products:list:{categoryId}:*
            // But for simplicity and to avoid stale data, we clear all product lists
        }
    }
}
