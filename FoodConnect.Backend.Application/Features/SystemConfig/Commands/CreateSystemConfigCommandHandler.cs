using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.SystemConfig.Commands;

public class CreateSystemConfigCommandHandler : IRequestHandler<CreateSystemConfigCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public CreateSystemConfigCommandHandler(
        ISystemConfigRepository systemConfigRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _systemConfigRepository = systemConfigRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(CreateSystemConfigCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        if(request.Type != (int)SystemConfigTypeEnum.Banner && request.ConfigValue == null)
        {
            return result.BuildFail("Config value is required");
        }

        if (request.Type != (int)SystemConfigTypeEnum.Banner)
        {
            var existingConfig = await _systemConfigRepository.GetByKeyAsync(request.ConfigKey);
            if (existingConfig != null)
                return result.BuildConflict($"Config with key '{request.ConfigKey}' already exists");
        }
        var uploadFileUrl = string.Empty;
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Upload banner image if provided
            string? bannerImageUrl = null;
            if (request.Type == (int)SystemConfigTypeEnum.Banner && request.BannerImage != null)
            {
                bannerImageUrl = await _fileStorageService.UploadFileAsync(request.BannerImage, "banners");
                uploadFileUrl = bannerImageUrl;
            }

            // Auto-adjust display order for Banner type
            if (request.Type == (int)SystemConfigTypeEnum.Banner && request.DisplayOrder.HasValue)
            {
                var existingBanners = await _systemConfigRepository
                    .GetConfigsByTypeAsync(request.Type);
                
                // Shift banners with displayOrder >= target up by 1
                foreach (var banner in existingBanners
                    .Where(b => (b.DisplayOrder ?? int.MaxValue) >= request.DisplayOrder.Value)
                    .OrderByDescending(b => b.DisplayOrder))
                {
                    banner.DisplayOrder = (banner.DisplayOrder ?? 0) + 1;
                    banner.UpdatedAtUtc = DateTime.UtcNow;
                    banner.UpdatedBy = userId;
                    _systemConfigRepository.Update(banner);
                }
            }

            // If IsActive = true, deactivate other configs of the same type
            if (request.IsActive && request.Type != (int)SystemConfigTypeEnum.Other)
            {
                var existingConfigsOfType = await _systemConfigRepository.GetConfigsByTypeAsync(request.Type);
                foreach (var config in existingConfigsOfType.Where(c => c.IsActive))
                {
                    config.IsActive = false;
                    config.UpdatedAtUtc = DateTime.UtcNow;
                    config.UpdatedBy = userId;
                    _systemConfigRepository.Update(config);
                }
            }

            var newConfig = new Domain.Entities.SystemConfig
            {
                ConfigKey = request.ConfigKey,
                ConfigValue = bannerImageUrl ?? request.ConfigValue, 
                Description = request.Description,
                DataType = request.DataType,
                IsEditable = request.IsEditable,
                Type = (SystemConfigTypeEnum)request.Type,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder
            };

            await _systemConfigRepository.AddAsync(newConfig);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(transaction);

            return result.BuildSuccess(new CreateOrUpdateResponse
            {
                Id = newConfig.Id,
                IsSuccess = true
            }, "System config created successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // delete uploaded file if exists
            try 
            {
                if (!string.IsNullOrEmpty(uploadFileUrl))
                {
                    await _fileStorageService.DeleteFileAsync(uploadFileUrl);
                }
            }
            catch
            {
                
            }
            return result.BuildFail($"Failed to create system config: {ex.Message}");
        }
    }
}
