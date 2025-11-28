using MedAssist.Domain.Enums;

namespace MedAssist.Domain.Entities;

public class Message
{
    public Guid Id { get; init; }
    public Guid DialogId { get; init; }
    public MessageRole Role { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
