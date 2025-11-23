using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.SystemConfig.Commands;

public class CreateSystemConfigCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public string ConfigKey { get; set; } = string.Empty;
    public string? ConfigValue { get; set; } 
    public string? Description { get; set; }
    public string DataType { get; set; } = "String";
    public bool IsEditable { get; set; } = true;
    public int Type { get; set; }
    public bool IsActive { get; set; } = true;
    public int? DisplayOrder { get; set; }
    public IFormFile? BannerImage { get; set; } 
}
