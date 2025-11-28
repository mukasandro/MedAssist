using MedAssist.Application.Abstractions;

namespace MedAssist.Infrastructure.Security;

public class StubCurrentUserContext : ICurrentUserContext
{
    private static readonly Guid DefaultDoctorId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Guid GetCurrentUserId() => DefaultDoctorId;
}
