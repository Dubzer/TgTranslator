using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Controllers;

namespace TgTranslator.Services
{
    public class TelegramBotHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TelegramBotClient _client;

        public TelegramBotHostedService(IServiceScopeFactory scopeFactory, TelegramBotClient client)
        {
            _scopeFactory = scopeFactory;
            _client = client;

            #region Events

            _client.OnMessage += OnMessage;
            _client.OnCallbackQuery += OnCallbackQuery;
            _client.OnReceiveError += OnReceiveError;
            _client.OnReceiveGeneralError += OnReceiveGeneralError;

            #endregion
            Program.Username = client.GetMeAsync().Result.Username;
        }

        #region IHostedService Members

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.DeleteWebhookAsync(cancellationToken);
            _client.StartReceiving(cancellationToken: cancellationToken, allowedUpdates: new[] {UpdateType.Message, UpdateType.CallbackQuery});
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                var controller = scope.ServiceProvider.GetRequiredService<TelegramBotController>();
            
                await controller.Post(new Update {Message = e.Message});
            }
            catch (Exception exception)
            {
                Log.Error(exception, "OnMessage: An unhandled exception");
            }
        }

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                var controller = scope.ServiceProvider.GetRequiredService<TelegramBotController>();

                await controller.Post(new Update {CallbackQuery = e.CallbackQuery});
            }
            catch (Exception exception)
            {
                Log.Error(exception, "OnCallbackQuery: An unhandled exception");
            }
        }

        private void OnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            Log.Error(e.ApiRequestException, $"OnReceiveError: {e.ApiRequestException.ErrorCode} - {e.ApiRequestException.Message}");
        }

        private void OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
        {
            Log.Error(e.Exception, nameof(OnReceiveGeneralError));
        }
    }
}