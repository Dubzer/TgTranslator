using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sentry;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Exceptions;
using TgTranslator.Extensions;
using TgTranslator.Interfaces;
using TgTranslator.Services;
using TgTranslator.Services.Handlers;

namespace TgTranslator.Controllers;

[Route("api/bot")]
[ServiceFilter(typeof(IpWhitelist))]
public class TelegramBotController : Controller
{
    private readonly ICallbackQueryHandler _callbackQueryHandler;
    private readonly MyChatMemberHandler _myChatMemberHandler;
    private readonly TelegramBotClient _client;
    private readonly IMessageHandler _messageHandler;
    private readonly GroupsBlacklistService _groupsBlacklist;

    public TelegramBotController(TelegramBotClient client, IMessageHandler messageHandler, ICallbackQueryHandler callbackQueryHandler, MyChatMemberHandler myChatMemberHandler, GroupsBlacklistService groupsBlacklist)
    {
        _client = client;
        _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        _callbackQueryHandler = callbackQueryHandler ?? throw new ArgumentNullException(nameof(callbackQueryHandler));
        _myChatMemberHandler = myChatMemberHandler;
        _groupsBlacklist = groupsBlacklist;
    }

    [HttpGet]
    public IActionResult Get() => Ok();

    [HttpPost]
    public async Task<OkResult> Post([FromBody] Update update)
    {
        if (update == null)
            return Ok();

        var updateTransaction = SentrySdk.StartTransaction(
            "update",
            $"update-{update.Type.ToString().ToLowerInvariant()}"
        );
        SentrySdk.ConfigureScope(sentryScope => sentryScope.Transaction = updateTransaction);

        switch (update.Type)
        {
            case UpdateType.Message:
                await OnMessage(update.Message);
                break;
            case UpdateType.CallbackQuery:
                await OnCallbackQuery(update.CallbackQuery);
                break;
            case UpdateType.MyChatMember:
                await OnMyChatMember(update.MyChatMember);
                break;
        }

        updateTransaction.Finish();
        return Ok();
    }
    
    private async Task OnMyChatMember(ChatMemberUpdated updateMyChatMember)
    {
        await _myChatMemberHandler.Handle(updateMyChatMember);
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
            Log.Error(exception, "Got a CallbackQuery with unsupported item");
        }
    }

    private async Task OnMessage(Message message)
    {
        if (message.Date < Program.StartedTime - TimeSpan.FromSeconds(10))
            return;

        try
        {
            await _messageHandler.HandleMessageAsync(message);
        }
        catch (InvalidSettingException)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, "It seems that this setting is not supported", replyToMessageId: message.MessageId);
        }
        catch (InvalidSettingValueException)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, "It seems that this value is not supported", replyToMessageId: message.MessageId);
        }
        catch (UnauthorizedSettingChangingException)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, "Hey! Only admins can change settings of this bot!",
                replyToMessageId: message.MessageId);
        }
        catch (ApiRequestException exception) when (exception.Message == "Bad Request: have no rights to send a message")
        {
            await _groupsBlacklist.AddGroup(message.Chat.Id);
        }
        catch (ApiRequestException exception) when (exception.Message.Contains("message not found", StringComparison.InvariantCultureIgnoreCase)) { }
        catch (ApiRequestException exception) when (exception.Message.Contains("Too Many Requests", StringComparison.InvariantCultureIgnoreCase)) { }
    }
}