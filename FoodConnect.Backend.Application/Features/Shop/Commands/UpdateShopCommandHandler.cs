using AutoMapper;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using System.Text.Json;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class UpdateShopCommandHandler : IRequestHandler<UpdateShopCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UpdateShopCommandHandler(
            IShopRepository shopRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _shopRepository = shopRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(UpdateShopCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized("User not found");
            }

            // Get existing shop
            var shop = await _shopRepository.GetDetailByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            // Check ownership
            if (shop.UserId != userId)
            {
                return result.BuildForbidden("You don't have permission to update this shop");
            }

            // CRITICAL: Only allow update if status is Draft
            if (shop.Status != ShopStatusEnum.Draft)
            {
                return result.BuildFail("Only shops in Draft status can be updated");
            }

            // Validate categories if provided
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                foreach (var categoryId in request.CategoryIds)
                {
                    var category = await _categoryRepository.GetByIdAsync(categoryId);
                    if (category == null)
                    {
                        return result.BuildNotFound($"Category with ID {categoryId} not found");
                    }
                }
            }

            // Validate payout method if provided
            if (request.PayoutMethod.HasValue)
            {
                if (!Enum.IsDefined(typeof(PaymentMethodEnum), request.PayoutMethod.Value))
                {
                    return result.BuildFail("Invalid payout method");
                }
            }

            var uploadedFiles = new List<string>();
            var filesToDelete = new List<string>();
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Use AutoMapper to update basic fields (only non-null properties)
                _mapper.Map(request, shop);

                // Delete assets if requested
                if (request.AssetIdsToDelete != null && request.AssetIdsToDelete.Any())
                {
                    var assetsToDelete = shop.Assets.Where(a => request.AssetIdsToDelete.Contains(a.Id)).ToList();
                    foreach (var asset in assetsToDelete)
                    {
                        filesToDelete.Add(asset.AssetUrl);
                        shop.Assets.Remove(asset);
                        
                        // Clear LogoUrl/CoverImageUrl if deleting those assets
                        if (asset.AssetType == ShopAssetTypeEnum.Logo)
                        {
                            shop.LogoUrl = null;
                        }
                        else if (asset.AssetType == ShopAssetTypeEnum.CoverImage)
                        {
                            shop.CoverImageUrl = null;
                        }
                    }
                }

                var prefix = $"{AWSDirectoryConstant.IMAGE_SHOP}/{shop.Id}";

                if (request.IdCardFront != null)
                {
                    var oldIdCardFront = shop.Assets.FirstOrDefault(a => a.AssetType == ShopAssetTypeEnum.IdCardFront);
                    if (oldIdCardFront != null)
                    {
                        shop.Assets.Remove(oldIdCardFront);
                        filesToDelete.Add(oldIdCardFront.AssetUrl);
                    }
                    
                    var url = await _fileStorageService.UploadFileAsync(request.IdCardFront, prefix);
                    uploadedFiles.Add(url);
                    
                    shop.Assets.Add(new Domain.Entities.ShopAsset
                    {
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.IdCardFront
                    });
                }

                if (request.IdCardBack != null)
                {
                    var oldIdCardBack = shop.Assets.FirstOrDefault(a => a.AssetType == ShopAssetTypeEnum.IdCardBack);
                    if (oldIdCardBack != null)
                    {
                        shop.Assets.Remove(oldIdCardBack);
                        filesToDelete.Add(oldIdCardBack.AssetUrl);
                    }
                    
                    var url = await _fileStorageService.UploadFileAsync(request.IdCardBack, prefix);
                    uploadedFiles.Add(url);
                    
                    shop.Assets.Add(new Domain.Entities.ShopAsset
                    {
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.IdCardBack
                    });
                }

                if (request.PortraitPhoto != null)
                {
                    var oldPortrait = shop.Assets.FirstOrDefault(a => a.AssetType == ShopAssetTypeEnum.PortraitPhoto);
                    if (oldPortrait != null)
                    {
                        shop.Assets.Remove(oldPortrait);
                        filesToDelete.Add(oldPortrait.AssetUrl);
                    }
                    
                    var url = await _fileStorageService.UploadFileAsync(request.PortraitPhoto, prefix);
                    uploadedFiles.Add(url);
                    
                    shop.Assets.Add(new Domain.Entities.ShopAsset
                    {
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.PortraitPhoto
                    });
                }

                if (request.KitchenPhotos != null && request.KitchenPhotos.Any())
                {
                    foreach (var kitchenPhoto in request.KitchenPhotos)
                    {
                        var url = await _fileStorageService.UploadFileAsync(kitchenPhoto, prefix);
                        uploadedFiles.Add(url);
                        
                        shop.Assets.Add(new Domain.Entities.ShopAsset
                        {
                            ShopId = shop.Id,
                            AssetUrl = url,
                            AssetType = ShopAssetTypeEnum.KitchenPhoto
                        });
                    }
                }

                if (request.FoodSafetyCertificate != null)
                {
                    var oldCertificate = shop.Assets.FirstOrDefault(a => a.AssetType == ShopAssetTypeEnum.FoodSafetyCertificate);
                    if (oldCertificate != null)
                    {
                        shop.Assets.Remove(oldCertificate); // Hard delete
                        filesToDelete.Add(oldCertificate.AssetUrl);
                    }
                    
                    var url = await _fileStorageService.UploadFileAsync(request.FoodSafetyCertificate, prefix);
                    uploadedFiles.Add(url);
                    
                    shop.Assets.Add(new Domain.Entities.ShopAsset
                    {
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.FoodSafetyCertificate
                    });
                }

                if (request.Logo != null)
                {
                    // Delete old Logo asset
                    var oldLogo = shop.Assets.FirstOrDefault(a => a.AssetType == ShopAssetTypeEnum.Logo);
                    if (oldLogo != null)
                    {
                        shop.Assets.Remove(oldLogo); // Hard delete
                        filesToDelete.Add(oldLogo.AssetUrl);
                    }
                    
                    var url = await _fileStorageService.UploadFileAsync(request.Logo, prefix);
                    uploadedFiles.Add(url);
                    shop.LogoUrl = url;
                    
                    shop.Assets.Add(new Domain.Entities.ShopAsset
                    {
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.Logo
                    });
                }

                if (request.CoverImage != null)
                {
                    var oldCoverImage = shop.Assets.FirstOrDefault(a => a.AssetType == ShopAssetTypeEnum.CoverImage);
                    if (oldCoverImage != null)
                    {
                        shop.Assets.Remove(oldCoverImage); // Hard delete
                        filesToDelete.Add(oldCoverImage.AssetUrl);
                    }
                    
                    var url = await _fileStorageService.UploadFileAsync(request.CoverImage, prefix);
                    uploadedFiles.Add(url);
                    shop.CoverImageUrl = url;
                    
                    shop.Assets.Add(new Domain.Entities.ShopAsset
                    {
                        // EF Core will auto-generate Id via ValueGeneratedOnAdd()
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.CoverImage
                    });
                }

                if (request.CategoryIds != null && request.CategoryIds.Any())
                {
                    var existingCategoryIds = shop.ShopCategories.Select(sc => sc.CategoryId).ToList();
                    var newCategoryIds = request.CategoryIds;

                    var categoriesToRemove = shop.ShopCategories
                        .Where(sc => !newCategoryIds.Contains(sc.CategoryId))
                        .ToList();
                    
                    foreach (var category in categoriesToRemove)
                    {
                        shop.ShopCategories.Remove(category);
                    }

                    var categoriesToAdd = newCategoryIds
                        .Where(cid => !existingCategoryIds.Contains(cid))
                        .ToList();
                    
                    foreach (var categoryId in categoriesToAdd)
                    {
                        shop.ShopCategories.Add(new Domain.Entities.ShopCategory
                        {
                            ShopId = shop.Id,
                            CategoryId = categoryId
                        });
                    }
                }

                if (!string.IsNullOrEmpty(request.OperatingHoursJson))
                {
                    try
                    {
                        var operatingHours = JsonSerializer.Deserialize<List<OperatingHourDto>>(request.OperatingHoursJson);
                        if (operatingHours != null)
                        {
                            var existingHours = shop.OperatingHours.ToList();
                            var requestedDays = operatingHours.Select(oh => oh.DayOfWeek).ToList();

                            var hoursToRemove = existingHours
                                .Where(existing => !requestedDays.Contains(existing.DayOfWeek))
                                .ToList();

                            foreach (var hour in hoursToRemove)
                            {
                                shop.OperatingHours.Remove(hour);
                            }

                            foreach (var oh in operatingHours)
                            {
                                var existing = existingHours.FirstOrDefault(e => e.DayOfWeek == oh.DayOfWeek);

                                if (existing != null)
                                {
                                    existing.OpenTime = TimeOnly.Parse(oh.OpenTime);
                                    existing.CloseTime = TimeOnly.Parse(oh.CloseTime);
                                }
                                else
                                {
                                    shop.OperatingHours.Add(new Domain.Entities.ShopOperatingHour
                                    {
                                        ShopId = shop.Id,
                                        DayOfWeek = oh.DayOfWeek,
                                        OpenTime = TimeOnly.Parse(oh.OpenTime),
                                        CloseTime = TimeOnly.Parse(oh.CloseTime)
                                    });
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignore invalid JSON
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                if (filesToDelete.Any())
                {
                    foreach (var fileUrl in filesToDelete)
                    {
                        try
                        {
                            await _fileStorageService.DeleteFileAsync(fileUrl);
                        }
                        catch
                        {
                        }
                    }
                }

                return result.BuildSuccess(new CreateOrUpdateResponse
                {
                    Id = shop.Id,
                    IsSuccess = true
                }, "Shop updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                
                // Cleanup newly uploaded files
                if (uploadedFiles.Any())
                {
                    foreach (var fileUrl in uploadedFiles)
                    {
                        try
                        {
                            await _fileStorageService.DeleteFileAsync(fileUrl);
                        }
                        catch
                        {
                        }
                    }
                }

                return result.BuildFail($"Failed to update shop: {ex.Message}");
            }
        }
    }
}
