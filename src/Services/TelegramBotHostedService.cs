﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentry;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgTranslator.Services;

public class TelegramBotHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotClient _client;

    public TelegramBotHostedService(IServiceScopeFactory scopeFactory, TelegramBotClient client)
    {
        _scopeFactory = scopeFactory;
        _client = client;

        Program.Username = client.GetMeAsync().GetAwaiter().GetResult().Username;
        SentrySdk.ConfigureScope(scope =>
        {
            scope.Contexts["bot"] = new
            {
                Program.Username
            };
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.DeleteWebhookAsync(cancellationToken: cancellationToken);
        _client.StartReceiving(cancellationToken: cancellationToken,
            updateHandler: UpdateHandler,
            pollingErrorHandler: ErrorHandler, receiverOptions: new()
            {
                Offset = -1,
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery,
                    UpdateType.InlineQuery,
                    UpdateType.ChatMember,
                    UpdateType.MyChatMember
                }
            });

        await _client.SetMyCommandsAsync(new[]
        {
            new BotCommand
            {
                Command = "contact",
                Description = "📩 Contact the developer"
            },
            new BotCommand
            {
                Command = "help",
                Description = "❔ How to add the bot"
            },
            new BotCommand
            {
                Command = "settings",
                Description = "⚙️ Change language and mode"
            },

        }, BotCommandScope.AllPrivateChats(), cancellationToken: cancellationToken);

        await _client.SetMyCommandsAsync(new[]
        {
            BotCommands.AdminCommand
        }, BotCommandScope.AllChatAdministrators(), cancellationToken: cancellationToken);
    }

    private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var router = scope.ServiceProvider.GetRequiredService<HandlersRouter>();
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