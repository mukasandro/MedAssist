namespace MedAssist.Domain.Entities;

public class StaticContent
{
    public Guid Id { get; init; }
    public string Code { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
}
