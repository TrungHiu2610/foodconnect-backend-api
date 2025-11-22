using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.SystemConfig;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.SystemConfig.Queries;

public class GetSystemConfigsQueryHandler : IRequestHandler<GetSystemConfigsQuery, BaseResponse<List<SystemConfigResponse>>>
{
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IMapper _mapper;

    public GetSystemConfigsQueryHandler(
        ISystemConfigRepository systemConfigRepository,
        IMapper mapper)
    {
        _systemConfigRepository = systemConfigRepository;
        _mapper = mapper;
    }

    public async Task<BaseResponse<List<SystemConfigResponse>>> Handle(GetSystemConfigsQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<List<SystemConfigResponse>>();

        List<Domain.Entities.SystemConfig> configs;

        if (!string.IsNullOrEmpty(request.ConfigKey))
        {
            var config = await _systemConfigRepository.GetByKeyAsync(request.ConfigKey);
            configs = config != null ? new List<Domain.Entities.SystemConfig> { config } : new List<Domain.Entities.SystemConfig>();
        }
        else if (request.Type.HasValue)
        {
            configs = await _systemConfigRepository.GetConfigsByTypeAsync(request.Type.Value);
        }
        else
        {
            configs = await _systemConfigRepository.GetAllConfigsAsync();
        }

        // ⭐ Order Banner type by DisplayOrder
        if (request.Type == (int)Domain.Enums.SystemConfigTypeEnum.Banner)
        {
            configs = configs.OrderBy(c => c.DisplayOrder ?? int.MaxValue).ToList();
        }

        var configResponses = _mapper.Map<List<SystemConfigResponse>>(configs);

        return result.BuildSuccess(configResponses, "System configs retrieved successfully");
    }
}
