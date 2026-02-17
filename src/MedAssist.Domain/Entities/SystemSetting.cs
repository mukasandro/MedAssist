namespace MedAssist.Domain.Entities;

public class SystemSetting
{
    public Guid Id { get; init; }
    public string Key { get; set; } = string.Empty;
    public string ValueJson { get; set; } = "{}";
    public DateTimeOffset UpdatedAt { get; set; }
}
