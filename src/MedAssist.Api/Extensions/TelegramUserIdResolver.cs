using System.Security.Claims;
using MedAssist.Api.Auth;
using Microsoft.AspNetCore.Mvc;

namespace MedAssist.Api.Extensions;

public static class TelegramUserIdResolver
{
    public static bool TryResolveTelegramUserId(this ControllerBase controller, long headerTelegramUserId, out long telegramUserId)
    {
        if (headerTelegramUserId > 0)
        {
            telegramUserId = headerTelegramUserId;
            return true;
        }

        var claimValue = controller.User.FindFirstValue(AuthClaimTypes.TelegramUserId);
        if (long.TryParse(claimValue, out var claimTelegramUserId) && claimTelegramUserId > 0)
        {
            telegramUserId = claimTelegramUserId;
            return true;
        }

        telegramUserId = 0;
        return false;
    }
}
