namespace MedAssist.Application.Abstractions;

public interface ICurrentUserContext
{
    Guid GetCurrentUserId();
}
