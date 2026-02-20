using MedAssist.Api.Extensions;
using MedAssist.Api.Swagger;
using MedAssist.Application.DTOs;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("bot")]
[Authorize(Policy = "MiniAppOrBot")]
[ApiController]
[Route("v1/me/chat")]
public class MeChatController : ControllerBase
{
    private readonly IBotChatService _botChatService;

    public MeChatController(IBotChatService botChatService)
    {
        _botChatService = botChatService;
    }

    [HttpGet("conversations")]
    [SwaggerOperation(
        Summary = "Мои диалоги",
        Description = "Возвращает список диалогов текущего пользователя.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BotConversationHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Conversations(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryResolveTelegramUserId(telegramUserId, out telegramUserId))
        {
            return BadRequest(new { error = "X-Telegram-User-Id header or JWT claim telegram_user_id is required." });
        }

        var result = await _botChatService.GetConversationsAsync(telegramUserId, take, cancellationToken);
        return Ok(result);
    }

    [HttpGet("conversations/{conversationId:guid}/turns")]
    [SwaggerOperation(
        Summary = "Мои сообщения в диалоге",
        Description = "Возвращает сообщения конкретного диалога текущего пользователя.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BotChatTurnHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Turns(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        Guid conversationId,
        [FromQuery] int take = 200,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryResolveTelegramUserId(telegramUserId, out telegramUserId))
        {
            return BadRequest(new { error = "X-Telegram-User-Id header or JWT claim telegram_user_id is required." });
        }

        var result = await _botChatService.GetTurnsAsync(telegramUserId, conversationId, take, cancellationToken);
        return Ok(result);
    }
}
