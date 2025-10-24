using AutoMapper;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Commands
{
    public class CreateShopRegistrationCommandHandler : IRequestHandler<CreateShopRegistrationCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IShopRegistrationRepository _shopRegistrationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public CreateShopRegistrationCommandHandler(
            IShopRegistrationRepository shopRegistrationRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _shopRegistrationRepository = shopRegistrationRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }
       
        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(CreateShopRegistrationCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();
            var response = new CreateOrUpdateResponse();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildFail("User not found");
            }

            // Kiểm tra user tồn tại
            var user = await _userRepository.GetByIdAsync((Guid)userId);
            if (user == null)
            {
                return result.BuildFail("User not found");
            }

            // Kiểm tra xem user đã có đơn đăng ký pending hoặc approved chưa
            var existingRegistration = await _shopRegistrationRepository.GetPendingOrApprovedByUserIdAsync((Guid)userId);
            if (existingRegistration != null)
            {
                if (existingRegistration.Status == ShopRegistrationStatusEnum.Pending)
                {
                    return result.BuildFail("You already have a pending shop registration");
                }
                else if (existingRegistration.Status == ShopRegistrationStatusEnum.Approved)
                {
                    return result.BuildFail("You already have an approved shop registration");
                }
            }

            // Tạo đơn đăng ký bằng AutoMapper
            var registration = _mapper.Map<ShopRegistration>(request);
            registration.Id = Guid.NewGuid();
            registration.UserId = (Guid)userId;
            registration.Status = ShopRegistrationStatusEnum.Pending;
            registration.Assets = new List<ShopRegistrationAsset>();

            var uploadedFiles = new List<string>();
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var prefix = $"{AWSDirectoryConstant.IMAGE_SHOP_REGISTRATION}/{userId}/{registration.Id}";

                // Upload ID Card Front
                var idCardFrontUrl = await _fileStorageService.UploadFileAsync(request.IdCardFront, prefix);
                uploadedFiles.Add(idCardFrontUrl);
                registration.Assets.Add(new ShopRegistrationAsset
                {
                    Id = Guid.NewGuid(),
                    AssetUrl = idCardFrontUrl,
                    AssetType = ShopRegistrationAssetTypeEnum.IdCardFront,
                    SellerRegistrationId = registration.Id
                });

                // Upload ID Card Back
                var idCardBackUrl = await _fileStorageService.UploadFileAsync(request.IdCardBack, prefix);
                uploadedFiles.Add(idCardBackUrl);
                registration.Assets.Add(new ShopRegistrationAsset
                {
                    Id = Guid.NewGuid(),
                    AssetUrl = idCardBackUrl,
                    AssetType = ShopRegistrationAssetTypeEnum.IdCardBack,
                    SellerRegistrationId = registration.Id
                });

                // Upload Portrait Photo
                var portraitPhotoUrl = await _fileStorageService.UploadFileAsync(request.PortraitPhoto, prefix);
                uploadedFiles.Add(portraitPhotoUrl);
                registration.Assets.Add(new ShopRegistrationAsset
                {
                    Id = Guid.NewGuid(),
                    AssetUrl = portraitPhotoUrl,
                    AssetType = ShopRegistrationAssetTypeEnum.PortraitPhoto,
                    SellerRegistrationId = registration.Id
                });

                // Upload Kitchen Photos
                if (request.KitchenPhotos != null && request.KitchenPhotos.Any())
                {
                    foreach (var kitchenPhoto in request.KitchenPhotos)
                    {
                        var kitchenPhotoUrl = await _fileStorageService.UploadFileAsync(kitchenPhoto, prefix);
                        uploadedFiles.Add(kitchenPhotoUrl);
                        registration.Assets.Add(new ShopRegistrationAsset
                        {
                            Id = Guid.NewGuid(),
                            AssetUrl = kitchenPhotoUrl,
                            AssetType = ShopRegistrationAssetTypeEnum.KitchenPhoto,
                            SellerRegistrationId = registration.Id
                        });
                    }
                }

                // Upload Food Safety Certificate (optional)
                if (request.FoodSafetyCertificate != null)
                {
                    var foodSafetyCertUrl = await _fileStorageService.UploadFileAsync(request.FoodSafetyCertificate, prefix);
                    uploadedFiles.Add(foodSafetyCertUrl);
                    registration.Assets.Add(new ShopRegistrationAsset
                    {
                        Id = Guid.NewGuid(),
                        AssetUrl = foodSafetyCertUrl,
                        AssetType = ShopRegistrationAssetTypeEnum.FoodSafetyCertificate,
                        SellerRegistrationId = registration.Id
                    });
                }

                await _shopRegistrationRepository.AddAsync(registration);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                response.Id = registration.Id;
                response.IsSuccess = true;

                return result.BuildSuccess(response, "Shop registration created successfully");
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
                return result.BuildFail($"Create shop registration failed: {ex.Message}");
            }
        }
    }
}
