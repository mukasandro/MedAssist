using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record MessageDto(
    Guid Id,
    Guid DialogId,
    MessageRole Role,
    string Content,
    DateTimeOffset CreatedAt);
