using MedAssist.Api.Swagger;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("admin")]
[ApiController]
[Route("v1/static-content")]
public class StaticContentController : ControllerBase
{
    private readonly IStaticContentService _staticContentService;

    public StaticContentController(IStaticContentService staticContentService)
    {
        _staticContentService = staticContentService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Список статичных текстов", Description = "Все записи для админки (код, заголовок, текст).")]
    [ProducesResponseType(typeof(IReadOnlyCollection<StaticContentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var items = await _staticContentService.GetListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Создать статичный текст", Description = "Создает новую запись для выдачи через API по коду.")]
    [ProducesResponseType(typeof(StaticContentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStaticContentRequest request, CancellationToken cancellationToken)
    {
        var created = await _staticContentService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Обновить статичный текст", Description = "Редактирует код, заголовок или текст.")]
    [ProducesResponseType(typeof(StaticContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaticContentRequest request, CancellationToken cancellationToken)
    {
        var updated = await _staticContentService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Удалить статичный текст", Description = "Удаляет запись по id.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _staticContentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
