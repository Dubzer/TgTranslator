using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Exceptions;
using TgTranslator.Services.Handlers;

namespace TgTranslator.Controllers
{
    [Route("bot")]
    public class TelegramBotController : Controller
    {
        private readonly TelegramBotClient _client;
        private readonly IMessageHandler _messageHandler;
        private readonly ICallbackQueryHandler _callbackQueryHandler;

        public TelegramBotController(TelegramBotClient client, IMessageHandler messageHandler, ICallbackQueryHandler callbackQueryHandler)
        {
            _client = client;
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _callbackQueryHandler = callbackQueryHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            if (update == null)
                return Ok();

            switch (update.Type)
            {
                case UpdateType.Message: 
                    await OnMessage(update.Message);
                    break;
                case UpdateType.CallbackQuery:
                    await OnCallbackQuery(update.CallbackQuery);
                    break;
            }
            
            return Ok();
        }

        private async Task OnCallbackQuery(CallbackQuery callbackQuery)
        {
            try
            {
                await _callbackQueryHandler.HandleCallbackQueryAsync(callbackQuery);
            }
            catch (UnsupportedCommand exception)
            {
                Log.Error(exception, "Got a CallbackQuery with unsupported command");
            }
            catch (UnsupportedMenuItem exception)
            {
                Log.Error(exception, "Got a CallbackQuery with unsupported message");
            }
            catch (MessageIsNotModifiedException exception) { }
            
        }

        private async Task OnMessage(Message message)
        {
            try
            {
                await _messageHandler.HandleMessageAsync(message);
            }
            catch (ApiRequestException exception)
            {
                if (exception.Message.Contains("Bad Request: have no rights to send a message") && message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
                    await _client.LeaveChatAsync(message.Chat.Id);
            }
            catch (Exception exception)
            {
                Log.Error(exception,
                    message.Type == MessageType.Text
                        ? $"Got an exception while tried to handle Message: {message.Text} by {message.From.Id} from {message.Chat.Id}"
                        : "Got an exception while tried to handle Message");
            }

        }
    }
}