using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.SystemConfig.Commands;

public class DeleteSystemConfigCommandHandler : IRequestHandler<DeleteSystemConfigCommand, BaseResponse<CreateOrUpdateResponse>>
{
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSystemConfigCommandHandler(
        ISystemConfigRepository systemConfigRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _systemConfigRepository = systemConfigRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(DeleteSystemConfigCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<CreateOrUpdateResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var config = await _systemConfigRepository.GetByIdAsync(request.Id);
        if (config == null)
            return result.BuildNotFound("Config not found");

        if (!config.IsEditable)
            return result.BuildFail($"Config '{config.ConfigKey}' is not editable and cannot be deleted");

        try
        {
            _systemConfigRepository.Remove(config);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result.BuildSuccess(new CreateOrUpdateResponse
            {
                Id = config.Id,
                IsSuccess = true
            }, "System config deleted successfully");
        }
        catch (Exception ex)
        {
            return result.BuildFail($"Failed to delete system config: {ex.Message}");
        }
    }
}
