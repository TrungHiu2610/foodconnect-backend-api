using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.SystemConfig.Commands;

public class UpdateSystemConfigCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public Guid Id { get; set; }
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = "String";
    public bool IsEditable { get; set; } = true;
    public int Type { get; set; } = 99;
    public bool IsActive { get; set; } = true;
    public string? Url { get; set; }
    public int? DisplayOrder { get; set; }
    public IFormFile? BannerImage { get; set; } // Image file for Banner type
}
