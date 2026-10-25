using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sentry;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Exceptions;
using TgTranslator.Interfaces;
using TgTranslator.Services.EventHandlers.Messages;
using TgTranslator.Utils;
using TgTranslator.Utils.Extensions;

namespace TgTranslator.Services.EventHandlers;

public class EventRouter
{
    private readonly ICallbackQueryHandler _callbackQueryHandler;
    private readonly MyChatMemberHandler _myChatMemberHandler;
    private readonly TelegramBotClient _client;
    private readonly MessageRouter _messageRouter;
    private readonly EditedMessageHandler _editedMessageHandler;
    private readonly GroupsBlacklistService _groupsBlacklist;
    private readonly SlowModeFilter _slowModeFilter;
    private readonly ILogger<EventRouter> _logger;

    public EventRouter(TelegramBotClient client, MessageRouter messageRouter, ICallbackQueryHandler callbackQueryHandler, MyChatMemberHandler myChatMemberHandler, GroupsBlacklistService groupsBlacklist, ILogger<EventRouter> logger, EditedMessageHandler editedMessageHandler, SlowModeFilter slowModeFilter)
    {
        _client = client;
        _messageRouter = messageRouter;
        _callbackQueryHandler = callbackQueryHandler;
        _myChatMemberHandler = myChatMemberHandler;
        _groupsBlacklist = groupsBlacklist;
        _logger = logger;
        _editedMessageHandler = editedMessageHandler;
        _slowModeFilter = slowModeFilter;
    }

    public async Task HandleUpdate(Update update)
    {
        var message = update.Message ?? update.EditedMessage;
        var updateDate = update.Message?.Date ?? update.EditedMessage?.EditDate;
        var updateAgeMs = (DateTime.UtcNow - updateDate)?.TotalMilliseconds;
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["UpdateId"] = update.Id,
            ["UpdateType"] = update.Type,
            ["UserId"] = message?.From.Id,
            ["ChatId"] = message?.Chat.Id,
            ["MessageId"] = message?.MessageId,
            ["UpdateAgeAtHandlerStartMs"] = updateAgeMs
        });
        var processingTimer = Stopwatch.StartNew();

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
                    await _myChatMemberHandler.Handle(update.MyChatMember);
                    break;
                case UpdateType.EditedMessage:
                    await _editedMessageHandler.Handle(update.EditedMessage);
                    break;
            }

            updateTransaction.Finish();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while processing update {UpdateId} with type {UpdateType}", update?.Id, update?.Type);
            SentrySdk.CaptureException(e);
        }
        finally
        {
            if (processingTimer.ElapsedMilliseconds > 10000)
                _logger.LogWarning("Slow update processing took {ProcessingMs}ms", processingTimer.ElapsedMilliseconds);
        }
    }

    private async Task OnMessage(Message message)
    {
        if (message.Date.ToUniversalTime() < DateTime.UtcNow - TimeSpan.FromSeconds(30))
        {
            _logger.LogWarning("Skipping update because it's too old! {MessageDate} {CurrentDate}",
                message.Date,
                DateTime.UtcNow);

             return;
        }

        try
        {
            string responseText;
            try
            {
                await _messageRouter.HandleMessage(message);
                return;
            }
            catch (InvalidSettingException)
            {
                responseText = "It seems that this setting is not supported";
            }
            catch (InvalidSettingValueException)
            {
                responseText = "It seems that this value is not supported";
            }
            catch (UnauthorizedSettingChangingException)
            {
                responseText = "Hey! Only admins can change settings of this bot!";
            }

            var (replyParams, ephemeralParams) = TelegramUtils.OptionalEphemeralReply(message);

            await _client.SendMessage(
                message.Chat.Id,
                responseText,
                replyParameters: replyParams,
                ephemeralMessageParameters: ephemeralParams
            );
        }
        catch (ApiRequestException apiException)
        {
            if (apiException.ErrorCode == 429)
            {
                _logger.LogWarning("Telegram API rate limited while processing message; retry after {RetryAfterSeconds}s",
                    apiException.Parameters?.RetryAfter);

                if (message.Chat.Type == ChatType.Supergroup)
                {
                    try
                    {
                        if (await _slowModeFilter.CheckAfterRateLimit(message.Chat.Id))
                            await _groupsBlacklist.AddGroup(message.Chat.Id);
                    }
                    catch (Exception lookupException)
                    {
                        _logger.LogWarning(lookupException, "Could not blacklist slow-mode chat {ChatId}", message.Chat.Id);
                    }
                }

                return;
            }

            if (apiException.IsMessageNotFound())
            {
                _logger.LogInformation("Skipping message processing because Telegram could not find the message: {TelegramErrorCode} {TelegramDescription}",
                    apiException.ErrorCode, apiException.Message);
                return;
            }

            if (message.Chat.Type is ChatType.Group or ChatType.Supergroup && apiException.IsChatPermissionDenied())
            {
                await _groupsBlacklist.AddGroup(message.Chat.Id);
                _logger.LogWarning("Blacklisted chat after Telegram denied permission: {TelegramErrorCode} {TelegramDescription}",
                    apiException.ErrorCode, apiException.Message);
                return;
            }

            throw;
        }
    }

    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        try
        {
            await _callbackQueryHandler.HandleCallbackQueryAsync(callbackQuery);
        }
        catch (UnsupportedCommand exception)
        {
            _logger.LogError(exception, "Got a CallbackQuery with unsupported command");
        }
        catch (UnsupportedMenuItem exception)
        {
            _logger.LogError(exception, "Got a CallbackQuery with unsupported item");
        }
    }
}
