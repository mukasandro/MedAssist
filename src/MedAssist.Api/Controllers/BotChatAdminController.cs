using MedAssist.Api.Swagger;
using MedAssist.Application.DTOs;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("admin")]
[Authorize(Policy = "BotOnly")]
[ApiController]
[Route("v1/admin/chat-history")]
public class BotChatAdminController : ControllerBase
{
    private readonly IBotChatService _botChatService;

    public BotChatAdminController(IBotChatService botChatService)
    {
        _botChatService = botChatService;
    }

    [HttpGet("conversations")]
    [SwaggerOperation(
        Summary = "История диалогов (список)",
        Description = "Возвращает список conversation с метаданными.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BotConversationHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Conversations(
        [FromQuery] long? telegramUserId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _botChatService.GetConversationsAsync(telegramUserId, take, cancellationToken);
        return Ok(result);
    }

    [HttpGet("conversations/{conversationId:guid}/turns")]
    [SwaggerOperation(
        Summary = "История сообщений в диалоге",
        Description = "Возвращает сообщения конкретного conversation.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BotChatTurnHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Turns(
        Guid conversationId,
        [FromQuery] int take = 200,
        CancellationToken cancellationToken = default)
    {
        var result = await _botChatService.GetTurnsAsync(conversationId, take, cancellationToken);
        return Ok(result);
    }
}
