using MedAssist.Api.Swagger;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("bot")]
[ApiController]
[Route("v1/static-content")]
public class StaticContentBotController : ControllerBase
{
    private readonly IStaticContentService _staticContentService;

    public StaticContentBotController(IStaticContentService staticContentService)
    {
        _staticContentService = staticContentService;
    }

    [HttpGet("{code}")]
    [SwaggerOperation(Summary = "Статичный текст по коду", Description = "Возвращает текстовое значение для бота.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var value = await _staticContentService.GetValueAsync(code, cancellationToken);
        return value is null ? NotFound() : Ok(value);
    }
}
