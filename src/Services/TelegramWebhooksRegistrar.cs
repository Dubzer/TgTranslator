using System;
using System.Threading.Tasks;
using Flurl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
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

                client.DeleteWebhook()
                    .GetAwaiter().GetResult();

                client
                    .SetWebhook(domain.AppendPathSegments("api", "bot"),
                        allowedUpdates:
                        [
                            UpdateType.Message,
                            UpdateType.CallbackQuery,
                            UpdateType.InlineQuery,
                            UpdateType.ChatMember,
                            UpdateType.MyChatMember,
                            UpdateType.EditedMessage
                        ],
                        dropPendingUpdates: true).GetAwaiter().GetResult();

                var me = client.GetMe()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                Static.Username = me.Username;
                Static.BotId = me.Id;

                var commandsManager = scope.ServiceProvider.GetRequiredService<CommandsManager>();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await commandsManager.SetDefaultCommands();
                        await commandsManager.SetBotDescriptions();
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception, "Failed to update bot commands or descriptions");
                    }
                });
            }

            next(builder);
        };
}
