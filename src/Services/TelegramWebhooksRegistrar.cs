using System;
using Flurl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TgTranslator.Data.Options;

namespace TgTranslator.Services;

public class TelegramWebhooksRegistrar : IStartupFilter
{
    private readonly IServiceProvider _serviceProvider;

    public TelegramWebhooksRegistrar(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
        
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var client = scope.ServiceProvider.GetRequiredService<TelegramBotClient>();
                var domain = scope.ServiceProvider.GetRequiredService<IOptions<TelegramOptions>>().Value.WebhooksDomain;

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
                Program.Username = me.Username;
                Program.BotId = me.Id;
            }

            next(builder);
        };
    }
}