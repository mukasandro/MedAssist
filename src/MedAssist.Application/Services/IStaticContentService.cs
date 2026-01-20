using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IStaticContentService
{
    Task<IReadOnlyCollection<StaticContentDto>> GetListAsync(CancellationToken cancellationToken);
    Task<StaticContentDto> CreateAsync(CreateStaticContentRequest request, CancellationToken cancellationToken);
    Task<StaticContentDto?> UpdateAsync(Guid id, UpdateStaticContentRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<string?> GetValueAsync(string code, CancellationToken cancellationToken);
}
