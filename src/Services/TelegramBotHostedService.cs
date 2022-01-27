using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Controllers;

namespace TgTranslator.Services;

public class TelegramBotHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotClient _client;

    public TelegramBotHostedService(IServiceScopeFactory scopeFactory, TelegramBotClient client)
    {
        _scopeFactory = scopeFactory;
        _client = client;
        Program.Username = client.GetMeAsync().Result.Username;
    }
        
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.DeleteWebhookAsync(cancellationToken: cancellationToken);
        _client.StartReceiving(cancellationToken: cancellationToken, 
            updateHandler: UpdateHandler,
            errorHandler: ErrorHandler, receiverOptions: new()
            {
                AllowedUpdates = new[] {UpdateType.Message, UpdateType.CallbackQuery}
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
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            var controller = scope.ServiceProvider.GetRequiredService<TelegramBotController>();
            await controller.Post(update);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "OnMessage: An unhandled exception");
        }
    }

    private Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        Log.Error(exception, "OnReceiveError");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

}