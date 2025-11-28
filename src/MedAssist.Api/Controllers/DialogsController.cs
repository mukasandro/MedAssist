using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Api.Swagger.Examples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Controllers;

[ApiController]
[Route("v1/dialogs")]
public class DialogsController : ControllerBase
{
    private readonly IDialogService _dialogService;

    public DialogsController(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [SwaggerOperation(Summary = "Создать диалог", Description = "Начать диалог с пациентом или без привязки.")]
    [HttpPost]
    [ProducesResponseType(typeof(DialogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerRequestExample(typeof(CreateDialogRequest), typeof(CreateDialogRequestExample))]
    public async Task<IActionResult> Create([FromBody] CreateDialogRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var dialog = await _dialogService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = dialog.Id }, dialog);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [SwaggerOperation(Summary = "Список диалогов", Description = "Все диалоги врача; можно фильтровать по patientId.")]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<DialogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] Guid? patientId, CancellationToken cancellationToken)
    {
        var dialogs = await _dialogService.GetListAsync(patientId, cancellationToken);
        return Ok(dialogs);
    }

    [SwaggerOperation(Summary = "Диалог по id", Description = "Подробности диалога.")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DialogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var dialog = await _dialogService.GetAsync(id, cancellationToken);
        return dialog is null ? NotFound() : Ok(dialog);
    }

    [SwaggerOperation(Summary = "Закрыть диалог", Description = "Перевести диалог в закрытое состояние.")]
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(DialogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var dialog = await _dialogService.CloseAsync(id, cancellationToken);
        return dialog is null ? NotFound() : Ok(dialog);
    }
}
