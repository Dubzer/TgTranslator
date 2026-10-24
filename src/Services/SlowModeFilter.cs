using System;
using System.Threading.Tasks;
using BitFaster.Caching;
using BitFaster.Caching.Lru;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TgTranslator.Services;

public class SlowModeFilter(TelegramBotClient client, ILogger<SlowModeFilter> logger)
{
    private readonly IAsyncCache<long, bool> _cache = new ConcurrentLruBuilder<long, bool>()
        .WithCapacity(10000)
        .WithExpireAfterWrite(TimeSpan.FromHours(1))
        .AsAsyncCache()
        .WithAtomicGetOrAdd()
        .Build();

    public async Task<bool> ShouldSkip(long chatId)
    {
        return await _cache.GetOrAddAsync(chatId, async id =>
        {
            var chat = await client.GetChat(id);
            if (chat.SlowModeDelay is not > 0)
                return false;

            var bot = await client.GetChatMember(id, Static.BotId);
            if (bot.Status == ChatMemberStatus.Administrator)
                return false;

            logger.LogInformation(
                "Skipping chat {ChatId}: slow mode is {SlowModeDelay}s and bot status is {BotStatus}",
                id, chat.SlowModeDelay, bot.Status);
            return true;
        });
    }

    public void Invalidate(long chatId)
    {
        _cache.TryRemove(chatId);
    }
}
