using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IStaticContentService
{
    Task<IReadOnlyCollection<StaticContentAdminDto>> GetListAsync(CancellationToken cancellationToken);
    Task<StaticContentAdminDto> CreateAsync(CreateStaticContentRequest request, CancellationToken cancellationToken);
    Task<StaticContentAdminDto?> UpdateAsync(Guid id, UpdateStaticContentRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<StaticContentDto?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
