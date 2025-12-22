using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.SystemConfig.Commands;

public class UpdateSystemConfigCommandHandler : IRequestHandler<UpdateSystemConfigCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public UpdateSystemConfigCommandHandler(
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

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(UpdateSystemConfigCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var config = await _systemConfigRepository.GetByIdAsync(request.Id);
        if (config == null)
            return result.BuildNotFound($"Config not found");

        if (!config.IsEditable)
            return result.BuildFail($"Config '{config.ConfigKey}' is not editable");

        if (request.DataType == "decimal" || request.DataType == "number")
        {
            if (!decimal.TryParse(request.ConfigValue, out _))
                return result.BuildFail($"Config value must be a valid {request.DataType}");
        }
        else if (request.DataType == "boolean")
        {
            if (!bool.TryParse(request.ConfigValue, out _))
                return result.BuildFail("Config value must be a valid boolean (true/false)");
        }

        var uploadedBannerImageUrl = string.Empty;

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            string? newBannerImageUrl = null;
            if (request.Type == (int)SystemConfigTypeEnum.Banner && request.BannerImage != null)
            {
                newBannerImageUrl = await _fileStorageService.UploadFileAsync(request.BannerImage, "banners");
                uploadedBannerImageUrl = newBannerImageUrl;

                if (!string.IsNullOrEmpty(config.ConfigValue) && config.ConfigValue.Contains("/"))
                {
                    try
                    {
                        var oldKey = config.ConfigValue.Split("/").Last();
                        await _fileStorageService.DeleteFileAsync(oldKey);
                    }
                    catch { /* Ignore deletion errors */ }
                }
            }

            if (request.Type == (int)SystemConfigTypeEnum.Banner && 
                config.DisplayOrder != request.DisplayOrder)
            {
                var oldOrder = config.DisplayOrder ?? 0;
                var newOrder = request.DisplayOrder ?? 0;
                
                var allBanners = await _systemConfigRepository
                    .GetConfigsByTypeAsync(request.Type);
                
                if (newOrder < oldOrder)
                {
                    foreach (var banner in allBanners
                        .Where(b => b.Id != request.Id && 
                                   (b.DisplayOrder ?? int.MaxValue) >= newOrder && 
                                   (b.DisplayOrder ?? int.MaxValue) < oldOrder)
                        .OrderByDescending(b => b.DisplayOrder))
                    {
                        banner.DisplayOrder = (banner.DisplayOrder ?? 0) + 1;
                        banner.UpdatedAtUtc = DateTime.UtcNow;
                        banner.UpdatedBy = userId;
                        _systemConfigRepository.Update(banner);
                    }
                }
                else if (newOrder > oldOrder)
                {
                    foreach (var banner in allBanners
                        .Where(b => b.Id != request.Id && 
                                   (b.DisplayOrder ?? int.MaxValue) > oldOrder && 
                                   (b.DisplayOrder ?? int.MaxValue) <= newOrder)
                        .OrderBy(b => b.DisplayOrder))
                    {
                        banner.DisplayOrder = (banner.DisplayOrder ?? 0) - 1;
                        banner.UpdatedAtUtc = DateTime.UtcNow;
                        banner.UpdatedBy = userId;
                        _systemConfigRepository.Update(banner);
                    }
                }
            }

            if (request.IsActive && request.Type != (int)SystemConfigTypeEnum.Other)
            {
                var existingConfigsOfType = await _systemConfigRepository.GetConfigsByTypeAsync(request.Type);
                foreach (var existingConfig in existingConfigsOfType.Where(c => c.IsActive && c.Id != request.Id))
                {
                    existingConfig.IsActive = false;
                    existingConfig.UpdatedAtUtc = DateTime.UtcNow;
                    existingConfig.UpdatedBy = userId;
                    _systemConfigRepository.Update(existingConfig);
                }
            }

            config.ConfigKey = request.ConfigKey;
            config.ConfigValue = newBannerImageUrl ?? request.ConfigValue; // Use new image URL if uploaded
            config.Description = request.Description;
            config.DataType = request.DataType;
            config.IsEditable = request.IsEditable;
            config.Type = (SystemConfigTypeEnum)request.Type;
            config.IsActive = request.IsActive;
            config.Url = request.Url;
            config.DisplayOrder = request.DisplayOrder;
            config.UpdatedAtUtc = DateTime.UtcNow;
            config.UpdatedBy = userId;

            _systemConfigRepository.Update(config);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(transaction);

            return result.BuildSuccess(new CreateOrUpdateResponse
            {
                Id = config.Id,
                IsSuccess = true
            }, "System config updated successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            if (!string.IsNullOrEmpty(uploadedBannerImageUrl))
            {
                try
                {
                    var key = uploadedBannerImageUrl.Split("/").Last();
                    await _fileStorageService.DeleteFileAsync(key);
                }
                catch {  }
            }
            return result.BuildFail($"Failed to update system config: {ex.Message}");
        }
    }
}
