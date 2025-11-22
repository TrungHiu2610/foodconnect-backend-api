using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities;

public class SystemConfig : BaseEntity
{
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = "String";
    public bool IsEditable { get; set; } = true;
    public SystemConfigTypeEnum Type { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Url { get; set; }
    public int? DisplayOrder { get; set; }
}
