using System;
using Flurl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Data.Options;

namespace TgTranslator.Services;

public class TelegramWebhooksRegistrar : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        builder =>
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var client = scope.ServiceProvider.GetRequiredService<TelegramBotClient>();
                var domain = scope.ServiceProvider.GetRequiredService<IOptions<TelegramOptions>>().Value.WebhooksDomain;

                client
                    .SetMyCommandsAsync(BotCommands.PrivateChatCommands, BotCommandScope.AllPrivateChats())
                    .ConfigureAwait(false)
                    .GetAwaiter().GetResult();

                client.DeleteWebhookAsync().GetAwaiter().GetResult();
                client
                    .SetWebhookAsync(domain.AppendPathSegments("api", "bot"),
                        allowedUpdates:new[]
                        {
                            UpdateType.Message,
                            UpdateType.CallbackQuery
                        },
                        dropPendingUpdates: true).GetAwaiter().GetResult();
                var me = client.GetMeAsync().GetAwaiter().GetResult();
                Static.Username = me.Username;
                Static.BotId = me.Id;
            }

            next(builder);
        };
}