using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Interfaces;
using MediatR;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Application.Commons.Constants;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Product.Commands
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, BaseResponse<UpdateProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductAssetRepository _productAssetRepository;
        private readonly IShopRepository _shopRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProductCommandHandler(IProductRepository productRepository, IShopRepository shopRepository,
            IProductAssetRepository productAssetRepository,
            ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper,
            IFileStorageService fileStorageService, ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _productAssetRepository = productAssetRepository;
            _shopRepository = shopRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _currentUserService = currentUserService;
        }
        public async Task<BaseResponse<UpdateProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<UpdateProductResponse>();
            var response = new UpdateProductResponse();

            var userId = _currentUserService.UserId;

            // validate user
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

            // validate category
            if (request.CategoryId.HasValue && request.CategoryId != Guid.Empty)
            {
                var category = await _categoryRepository.GetByIdAsync((Guid)request.CategoryId);
                if (category == null)
                {
                    return result.BuildFail("Category not found");
                }
            }

            // validate product
            var product = await _productRepository.GetProductWithAssetsAsync(request.Id, tracking: true);

            if (product == null)
            {
                return result.BuildFail("Product not found");
            }
            if (product.ShopId != shop.Id)
            {
                return result.BuildFail("This product does not belong to your shop");
            }

            var uploadedFiles = new List<string>();
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // update some fields of product
                _mapper.Map(request, product);

                // delete product assets
                var productAssetsToDelete = new List<ProductAsset>();
                var urlsToDeleteFromS3 = new List<string>();

                if (request.RemovedProductAssetIds != null && request.RemovedProductAssetIds.Any())
                {
                    var thumbnailTrueCountInDb = product.ProductAssets.Count(pa => pa.IsThumbnail == true);
                    var thumbnailTrueCountNewRequest = (request.NewProductAssets == null || !request.NewProductAssets.Any()) ? 0 : request.NewProductAssets.Count(pa => pa.IsThumbnail == true);

                    // validate thumbnail in case we delete a thumbnail and there is no new thumbnail in the new request
                    var removingThumbnails = product.ProductAssets.Count(pa => pa.IsThumbnail == true && request.RemovedProductAssetIds.Contains(pa.Id));
                    if (thumbnailTrueCountInDb - removingThumbnails + thumbnailTrueCountNewRequest < 1)
                    {
                        await _unitOfWork.RollbackTransactionAsync(transaction);
                        return result.BuildFail("At least one thumbnail is required.");
                    }
                    foreach (var productAssetId in request.RemovedProductAssetIds)
                    {
                        var productAsset = product.ProductAssets.FirstOrDefault(pa => pa.Id == productAssetId);
                        if (productAsset != null)
                        {
                            productAssetsToDelete.Add(productAsset);
                            urlsToDeleteFromS3.Add(productAsset.AssetUrl);
                        }
                    }
                    productAssetsToDelete.ForEach(pa => product.ProductAssets.Remove(pa));
                }

                // update product assets
                if (request.UpdateProductAssets != null && request.UpdateProductAssets.Any())
                {
                    // validate thumbnail
                    var thumbnailTrueCountRequest = request.UpdateProductAssets.Count(pa => pa.IsThumbnail == true);
                    var thumbnailFalseCountRequest = request.UpdateProductAssets.Count(pa => pa.IsThumbnail == false);
                    var thumbnailTrueCountInDb = product.ProductAssets.Count(pa => pa.IsThumbnail == true);
                    var thumbnailTrueCountNewRequest = (request.NewProductAssets == null || !request.NewProductAssets.Any()) ? 0 : request.NewProductAssets.Count(pa => pa.IsThumbnail == true);

                    if (thumbnailTrueCountRequest > 1)
                    {
                        await _unitOfWork.RollbackTransactionAsync(transaction);
                        return result.BuildFail("Only one thumbnail is allowed.");
                    }
                    // validate in case we update false to true for a thumbnail
                    if (thumbnailFalseCountRequest == request.UpdateProductAssets.Count() && thumbnailTrueCountNewRequest == 0
                        && thumbnailTrueCountInDb == 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync(transaction);
                        return result.BuildFail("At least one thumbnail is required.");
                    }

                    // Update asset metadata (description only)
                    var updatedAssetsDict = request.UpdateProductAssets.ToDictionary(a => a.Id);
                    var newThumbnailId = request.UpdateProductAssets.FirstOrDefault(a => a.IsThumbnail == true)?.Id;
                    
                    foreach (var assetInDb in product.ProductAssets)
                    {
                        if (updatedAssetsDict.TryGetValue(assetInDb.Id, out var assetUpdateInfo))
                        {
                            assetInDb.AssetDescription = assetUpdateInfo.AssetDescription;
                        }
                        // set thumbnail 
                        if (newThumbnailId != null)
                        {
                            assetInDb.IsThumbnail = (assetInDb.Id == newThumbnailId);
                        }
                    }
                }

                // add new product assets
                if (request.NewProductAssets != null && request.NewProductAssets.Any())
                {
                    // validate thumbnail
                    var thumbnailTrueCountRequest = request.NewProductAssets.Count(pa => pa.IsThumbnail == true);
                    var thumbnailTrueCountInDb = product.ProductAssets.Count(pa => pa.IsThumbnail == true);
                    if (thumbnailTrueCountRequest + thumbnailTrueCountInDb > 1)
                    {
                        await _unitOfWork.RollbackTransactionAsync(transaction);
                        return result.BuildFail("Only one thumbnail is allowed.");
                    }
                    foreach (var assetDto in request.NewProductAssets)
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
                    
                    // Map DTOs to entities and add to product collection
                    var newAssets = _mapper.Map<ICollection<ProductAsset>>(request.NewProductAssets);
                    foreach (var newAsset in newAssets)
                    {
                        newAsset.ProductId = product.Id;
                        product.ProductAssets.Add(newAsset);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                // delete product assets from S3 for those deleted assets
                if (urlsToDeleteFromS3.Any())
                {
                    await _fileStorageService.DeleteFilesAsync(urlsToDeleteFromS3);
                }

                response.Id = product.Id;
                response.IsSuccess = true;

                return result.BuildSuccess(response, "Update product success");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction);

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
                return result.BuildFail("An error occurred while updating the product: " + ex.Message);
            }
        }
    }
}
