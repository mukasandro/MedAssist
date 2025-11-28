using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Api.Swagger.Examples;
using MedAssist.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Controllers;

[ApiController]
[Route("v1/dialogs/{dialogId:guid}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IDialogService _dialogService;

    public MessagesController(IMessageService messageService, IDialogService dialogService)
    {
        _messageService = messageService;
        _dialogService = dialogService;
    }

    [SwaggerOperation(Summary = "Лента сообщений", Description = "Сообщения диалога в хронологическом порядке.")]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid dialogId, CancellationToken cancellationToken)
    {
        var dialog = await _dialogService.GetAsync(dialogId, cancellationToken);
        if (dialog is null)
        {
            return NotFound();
        }

        var messages = await _messageService.GetListAsync(dialogId, cancellationToken);
        return Ok(messages);
    }

    [SwaggerOperation(Summary = "Добавить сообщение", Description = "Отправить реплику в диалог. Роль: doctor | assistant | system.")]
    [HttpPost]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(CreateMessageRequest), typeof(CreateMessageRequestExample))]
    public async Task<IActionResult> Post(Guid dialogId, [FromBody] CreateMessageRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MessageRole>(request.Role, true, out _))
        {
            return BadRequest(new { error = "Invalid role. Use doctor, assistant, or system." });
        }

        var dialog = await _dialogService.GetAsync(dialogId, cancellationToken);
        if (dialog is null)
        {
            return NotFound();
        }

        var message = await _messageService.AddAsync(dialogId, request, cancellationToken);
        if (message is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(Get), new { dialogId }, message);
    }
}
