using System.Collections.Generic;

namespace MedAssist.Application.Requests;

public record UpsertRegistrationRequest(
    IReadOnlyCollection<string>? SpecializationCodes,
    long TelegramUserId,
    string? Nickname);
