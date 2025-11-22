namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.SystemConfig;

public class SystemConfigResponse
{
    public Guid Id { get; set; }
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = string.Empty;
    public bool IsEditable { get; set; }
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Url { get; set; }
    public int? DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
