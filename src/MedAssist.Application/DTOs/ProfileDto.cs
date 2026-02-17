using System.Collections.Generic;
using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record ProfileDto(
    Guid DoctorId,
    IReadOnlyCollection<SpecializationDto> Specializations,
    long? TelegramUserId,
    int TokenBalance,
    string? Nickname,
    Guid? LastSelectedPatientId,
    string? LastSelectedPatientNickname);
