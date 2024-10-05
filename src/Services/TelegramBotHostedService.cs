using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentry;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Services.EventHandlers;

namespace TgTranslator.Services;

public class TelegramBotHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotClient _client;
    private readonly CommandsManager _commandsManager;

    public TelegramBotHostedService(IServiceScopeFactory scopeFactory, TelegramBotClient client, CommandsManager commandsManager)
    {
        _scopeFactory = scopeFactory;
        _client = client;
        _commandsManager = commandsManager;

        var me = client.GetMeAsync().GetAwaiter().GetResult();
        Static.Username = me.Username;
        Static.BotId = me.Id;

        SentrySdk.ConfigureScope(scope =>
        {
            scope.Contexts["bot"] = new
            {
                Static.Username
            };
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.StartReceiving(
            updateHandler: UpdateHandler,
            errorHandler: ErrorHandler,
            receiverOptions: new()
            {
                Offset = -1,
                AllowedUpdates =
                [
                    UpdateType.Message,
                    UpdateType.CallbackQuery,
                    UpdateType.InlineQuery,
                    UpdateType.ChatMember,
                    UpdateType.MyChatMember
                ]
            },
            cancellationToken: cancellationToken);

        await _commandsManager.SetDefaultCommands();
    }

    private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var router = scope.ServiceProvider.GetRequiredService<EventRouter>();
        await router.HandleUpdate(update);
    }

    private static Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        Log.Error(exception, "OnReceiveError");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}