using System.Collections.Generic;
using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record ProfileDto(
    Guid DoctorId,
    IReadOnlyCollection<string> SpecializationCodes,
    IReadOnlyCollection<string> SpecializationTitles,
    long? TelegramUserId,
    string? Nickname,
    Guid? LastSelectedPatientId,
    bool Verified);
