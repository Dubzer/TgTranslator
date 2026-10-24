using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TgTranslator.Services;

public class SlowModeFilter(TelegramBotClient client, ILogger<SlowModeFilter> logger)
{
    public async Task<bool> CheckAfterRateLimit(long chatId)
    {
        var chat = await client.GetChat(chatId);
        if (chat.SlowModeDelay is not > 0)
            return false;

        var bot = await client.GetChatMember(chatId, Static.BotId);
        if (bot.Status == ChatMemberStatus.Administrator)
            return false;

        logger.LogInformation(
            "Skipping chat {ChatId}: slow mode is {SlowModeDelay}s and bot status is {BotStatus}",
            chatId, chat.SlowModeDelay, bot.Status);
        return true;
    }
}
