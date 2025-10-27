using AutoMapper;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using System.Text.Json;
using FoodConnect.Backend.Application.Commons.DTOs;
using System;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class CreateShopCommandHandler : IRequestHandler<CreateShopCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrentUserService _currentUserService;

        public CreateShopCommandHandler(
            IShopRepository shopRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            ICurrentUserService currentUserService)
        {
            _shopRepository = shopRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(CreateShopCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized("User not found");
            }

            // Check if user already has a shop
            var existingShop = await _shopRepository.GetByUserIdAsync((Guid)userId);
            if (existingShop != null)
            {
                return result.BuildConflict("You already have a shop registered");
            }

            // Validate categories exist
            foreach (var categoryId in request.CategoryIds)
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId);
                if (category == null)
                {
                    return result.BuildNotFound($"Category with ID {categoryId} not found");
                }
            }

            // Parse payout method
            if (!Enum.IsDefined(typeof(PaymentMethodEnum), (int)request.PayoutMethod))
            {
                return result.BuildFail("Invalid payout method");
            }
            var payoutMethod = (PaymentMethodEnum)request.PayoutMethod;

            var shop = new Domain.Entities.Shop
            {
                ShopName = request.ShopName,
                Description = request.Description,
                Street = request.Street,
                Ward = request.Ward,
                District = request.District,
                City = request.City,
                Country = request.Country,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                PayoutMethod = payoutMethod,
                PayoutAccountInfo = request.PayoutAccountInfo,
                PayoutAccountName = request.PayoutAccountName,
                UserId = (Guid)userId,
                Status = ShopStatusEnum.Draft,
            };

            // Handle file uploads
            var uploadedFiles = new List<string>();
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var shopAssets = new List<Domain.Entities.ShopAsset>();

                // ID Card Front
                if (request.IdCardFront != null)
                {
                    var prefix = $"{AWSDirectoryConstant.IMAGE_SHOP}/{shop.Id}";
                    var url = await _fileStorageService.UploadFileAsync(request.IdCardFront, prefix);
                    uploadedFiles.Add(url);
                    
                    shopAssets.Add(new Domain.Entities.ShopAsset
                    {
                        Id = Guid.NewGuid(),
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.IdCardFront
                    });
                }

                // ID Card Back
                if (request.IdCardBack != null)
                {
                    var prefix = $"{AWSDirectoryConstant.IMAGE_SHOP}/{shop.Id}";
                    var url = await _fileStorageService.UploadFileAsync(request.IdCardBack, prefix);
                    uploadedFiles.Add(url);
                    
                    shopAssets.Add(new Domain.Entities.ShopAsset
                    {
                        Id = Guid.NewGuid(),
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.IdCardBack
                    });
                }

                // Portrait Photo
                if (request.PortraitPhoto != null)
                {
                    var prefix = $"{AWSDirectoryConstant.IMAGE_SHOP}/{shop.Id}";
                    var url = await _fileStorageService.UploadFileAsync(request.PortraitPhoto, prefix);
                    uploadedFiles.Add(url);
                    
                    shopAssets.Add(new Domain.Entities.ShopAsset
                    {
                        Id = Guid.NewGuid(),
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.PortraitPhoto
                    });
                }

                // Kitchen Photos
                if (request.KitchenPhotos != null && request.KitchenPhotos.Any())
                {
                    foreach (var kitchenPhoto in request.KitchenPhotos)
                    {
                        var prefix = $"{AWSDirectoryConstant.IMAGE_SHOP}/{shop.Id}";
                        var url = await _fileStorageService.UploadFileAsync(kitchenPhoto, prefix);
                        uploadedFiles.Add(url);
                        
                        shopAssets.Add(new Domain.Entities.ShopAsset
                        {
                            Id = Guid.NewGuid(),
                            ShopId = shop.Id,
                            AssetUrl = url,
                            AssetType = ShopAssetTypeEnum.KitchenPhoto
                        });
                    }
                }

                // Food Safety Certificate
                if (request.FoodSafetyCertificate != null)
                {
                    var prefix = $"{AWSDirectoryConstant.IMAGE_SHOP}/{shop.Id}";
                    var url = await _fileStorageService.UploadFileAsync(request.FoodSafetyCertificate, prefix);
                    uploadedFiles.Add(url);
                    
                    shopAssets.Add(new Domain.Entities.ShopAsset
                    {
                        Id = Guid.NewGuid(),
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.FoodSafetyCertificate
                    });
                }

                // Logo
                if (request.Logo != null)
                {
                    var prefix = $"{AWSDirectoryConstant.IMAGE_SHOP}/{shop.Id}";
                    var url = await _fileStorageService.UploadFileAsync(request.Logo, prefix);
                    uploadedFiles.Add(url);
                    shop.LogoUrl = url;
                    
                    shopAssets.Add(new Domain.Entities.ShopAsset
                    {
                        Id = Guid.NewGuid(),
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.Logo
                    });
                }

                // Cover Image
                if (request.CoverImage != null)
                {
                    var prefix = $"{AWSDirectoryConstant.IMAGE_SHOP}/{shop.Id}";
                    var url = await _fileStorageService.UploadFileAsync(request.CoverImage, prefix);
                    uploadedFiles.Add(url);
                    shop.CoverImageUrl = url;
                    
                    shopAssets.Add(new Domain.Entities.ShopAsset
                    {
                        Id = Guid.NewGuid(),
                        ShopId = shop.Id,
                        AssetUrl = url,
                        AssetType = ShopAssetTypeEnum.CoverImage
                    });
                }

                shop.Assets = shopAssets;

                // Add shop categories
                shop.ShopCategories = request.CategoryIds.Select(categoryId => new Domain.Entities.ShopCategory
                {
                    ShopId = shop.Id,
                    CategoryId = categoryId
                }).ToList();

                // Parse and add operating hours if provided
                if (!string.IsNullOrEmpty(request.OperatingHoursJson))
                {
                    try
                    {
                        var operatingHours = JsonSerializer.Deserialize<List<OperatingHourDto>>(request.OperatingHoursJson);
                        if (operatingHours != null)
                        {
                            shop.OperatingHours = operatingHours.Select(oh => new Domain.Entities.ShopOperatingHour
                            {
                                Id = Guid.NewGuid(),
                                ShopId = shop.Id,
                                DayOfWeek = oh.DayOfWeek,
                                OpenTime = TimeOnly.Parse(oh.OpenTime),
                                CloseTime = TimeOnly.Parse(oh.CloseTime)
                            }).ToList();
                        }
                    }
                    catch
                    {
                        // Ignore invalid JSON
                    }
                }

                await _shopRepository.AddAsync(shop);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return result.BuildSuccess(new CreateOrUpdateResponse
                {
                    Id = shop.Id,
                    IsSuccess = true
                }, "Shop created successfully. Status: Draft");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                
                // Cleanup uploaded files
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

                return result.BuildFail($"Failed to create shop: {ex.Message}");
            }
        }
    }
}
