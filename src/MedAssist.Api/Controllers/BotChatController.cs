using MedAssist.Api.Swagger;
using MedAssist.Api.Contracts.BotChat;
using MedAssist.Api.Extensions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("bot")]
[Authorize(Policy = "BotOnly")]
[ApiController]
[Route("v1/bot/chat")]
public class BotChatController : ControllerBase
{
    private readonly IBotChatService _botChatService;

    public BotChatController(IBotChatService botChatService)
    {
        _botChatService = botChatService;
    }

    [HttpPost("ask")]
    [SwaggerOperation(
        Summary = "Задать вопрос LLM через Telegram-бота",
        Description = "Создает или продолжает conversation и возвращает ответ ассистента.")]
    [ProducesResponseType(typeof(BotChatAnswerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Ask(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        [FromBody] AskBotQuestionHttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!this.TryResolveTelegramUserId(telegramUserId, out telegramUserId))
        {
            return BadRequest(new { error = "X-Telegram-User-Id header or JWT claim telegram_user_id is required." });
        }

        var serviceRequest = new AskBotQuestionRequest(
            TelegramUserId: telegramUserId,
            Text: request.Text,
            ConversationId: request.ConversationId,
            RequestId: request.RequestId);

        var answer = await _botChatService.AskAsync(serviceRequest, cancellationToken);
        return Ok(answer);
    }
}
