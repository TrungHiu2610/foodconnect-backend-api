using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.SystemConfig;
using MediatR;

namespace FoodConnect.Backend.Application.Features.SystemConfig.Queries;

public class GetSystemConfigsQuery : IRequest<BaseResponse<List<SystemConfigResponse>>>
{
    public string? ConfigKey { get; set; }
    public int? Type { get; set; }
}
