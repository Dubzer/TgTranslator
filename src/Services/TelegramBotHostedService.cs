using System;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Controllers;
using TgTranslator.Exceptions;
using TgTranslator.Services.Handlers;

namespace TgTranslator.Services
{
    public class TelegramBotHostedService : IHostedService
    {
        private readonly TelegramBotClient _client;
        private readonly TelegramBotController _controller; 

        public TelegramBotHostedService(TelegramBotClient client, TelegramBotController controller)
        {
            _client = client;
            _controller = controller;
            
            #region Events

            _client.OnMessage += OnMessage;
            _client.OnCallbackQuery += OnCallbackQuery;
            _client.OnReceiveError += OnReceiveError;
            _client.OnReceiveGeneralError += OnReceiveGeneralError;
    
            #endregion
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _client.StartReceiving(cancellationToken: cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            await _controller.Post(new Update { Message = e.Message});
        }
    
        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            await _controller.Post(new Update { CallbackQuery = e.CallbackQuery});
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
