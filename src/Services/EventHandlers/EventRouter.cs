using System;
using System.Threading.Tasks;
using Sentry;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Exceptions;
using TgTranslator.Interfaces;
using TgTranslator.Services.EventHandlers.Messages;
using TgTranslator.Utils;

namespace TgTranslator.Services.EventHandlers;

public class EventRouter
{
    private readonly ICallbackQueryHandler _callbackQueryHandler;
    private readonly MyChatMemberHandler _myChatMemberHandler;
    private readonly TelegramBotClient _client;
    private readonly MessageRouter _messageRouter;
    private readonly GroupsBlacklistService _groupsBlacklist;
    private readonly ILogger _logger;

    public EventRouter(TelegramBotClient client, MessageRouter messageRouter, ICallbackQueryHandler callbackQueryHandler, MyChatMemberHandler myChatMemberHandler, GroupsBlacklistService groupsBlacklist, ILogger logger)
    {
        _client = client;
        _messageRouter = messageRouter;
        _callbackQueryHandler = callbackQueryHandler;
        _myChatMemberHandler = myChatMemberHandler;
        _groupsBlacklist = groupsBlacklist;
        _logger = logger;
    }

    public async Task HandleUpdate(Update update)
    {
        try
        {
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
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error while processing update {UpdateId} with type {UpdateType}", update?.Id, update?.Type);
            SentrySdk.CaptureException(e);
        }
    }

    private async Task OnMessage(Message message)
    {
        if (message.Date.ToUniversalTime() < DateTime.UtcNow - TimeSpan.FromSeconds(30))
        {
            _logger.Warning("Skipping update because it's too old! {MessageDate} {CurrentDate}",
                message.Date,
                DateTime.UtcNow);

             return;
        }

        try
        {
            await _messageRouter.HandleMessage(message);
        }
        catch (InvalidSettingException)
        {
            await _client.SendTextMessageAsync(message.Chat.Id,
                "It seems that this setting is not supported",
                replyParameters: TelegramUtils.SafeReplyTo(message.MessageId));
        }
        catch (InvalidSettingValueException)
        {
            await _client.SendTextMessageAsync(message.Chat.Id,
                "It seems that this value is not supported",
                replyParameters: TelegramUtils.SafeReplyTo(message.MessageId));
        }
        catch (UnauthorizedSettingChangingException)
        {
            await _client.SendTextMessageAsync(message.Chat.Id,
                "Hey! Only admins can change settings of this bot!",
                replyParameters: TelegramUtils.SafeReplyTo(message.MessageId));
        }
        catch (ApiRequestException exception) when (exception.Message.Contains("CHAT_RESTRICTED")
                                                    || exception.Message.Contains("have no rights to send a message")
                                                    || exception.Message.Contains("not enough rights to"))
        {
            await _groupsBlacklist.AddGroup(message.Chat.Id);

        }
        catch (ApiRequestException exception) when (exception.Message.Contains("message not found", StringComparison.InvariantCultureIgnoreCase)) { }
        catch (ApiRequestException exception) when (exception.Message.Contains("Too Many Requests", StringComparison.InvariantCultureIgnoreCase)) { }
    }

    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        try
        {
            await _callbackQueryHandler.HandleCallbackQueryAsync(callbackQuery);
        }
        catch (UnsupportedCommand exception)
        {
            _logger.Error(exception, "Got a CallbackQuery with unsupported command");
        }
        catch (UnsupportedMenuItem exception)
        {
            _logger.Error(exception, "Got a CallbackQuery with unsupported item");
        }
    }

    private async Task OnMyChatMember(ChatMemberUpdated updateMyChatMember)
    {
        await _myChatMemberHandler.Handle(updateMyChatMember);
    }
}