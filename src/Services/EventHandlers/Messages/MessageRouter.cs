using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sentry;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Data.Options;
using TgTranslator.Menu;
using TgTranslator.Stats;
using TgTranslator.Utils.Extensions;
using TgTranslator.Validation;

namespace TgTranslator.Services.EventHandlers.Messages;

public class MessageRouter
{
    private readonly Blacklists _blacklist;
    private readonly BotMenu _botMenu;

    private readonly Metrics _metrics;
    private readonly SettingsService _settingsService;
    private readonly TranslateHandler _translateHandler;
    private readonly CommandHandler _commandHandler;
    private readonly SettingsChangeHandler _settingsChangeHandler;
    private readonly MessageValidator _validator;
    private readonly UsersDatabaseService _users;
    private readonly GroupsBlacklistService _groupsBlacklist;
    private readonly ILogger _logger;

    private static readonly FrozenSet<string> ManualTranslationCommands = new HashSet<string>
    {
        $"@{Static.Username}",
        "!translate",
        "/tl",
        $"/tl@{Static.Username}"
    }.ToFrozenSet();

    public MessageRouter(SettingsService settingsService, BotMenu botMenu,
        TranslateHandler translateHandler, CommandHandler commandHandler, SettingsChangeHandler settingsChangeHandler, Metrics metrics, IOptions<Blacklists> blacklistsOptions, MessageValidator validator, UsersDatabaseService users, GroupsBlacklistService groupsBlacklist, ILogger logger)
    {
        _settingsService = settingsService;
        _botMenu = botMenu;
        _translateHandler = translateHandler;
        _commandHandler = commandHandler;
        _settingsChangeHandler = settingsChangeHandler;
        _metrics = metrics;
        _blacklist = blacklistsOptions.Value;
        _validator = validator;
        _users = users;
        _groupsBlacklist = groupsBlacklist;
        _logger = logger;
    }

    public async Task HandleMessage(Message message)
    {
        _metrics.HandleMessage();
        _logger.Information("Got new message {ChatId} | {From} | {ChatType}", message.Chat.Id, message.From, message.Chat.Type);
        SentrySdk.ConfigureScope(scope =>
        {
            scope.User = new SentryUser
            {
                Id = message.From?.Id.ToString(),
                Username = message.From?.Username
            };
        });

        if (message.From != null && _blacklist.UserIdsBlacklist.Contains(message.From.Id))
            return;

        switch (message.Chat.Type)
        {
            case ChatType.Group:
            case ChatType.Supergroup:
                await HandleGroupMessage(message);
                break;

            case ChatType.Private:
                await HandlePrivateMessage(message);
                break;
        }
    }

    private async Task HandleGroupMessage(Message message)
    {
        string messageText = message.TextOrCaption();
        if (messageText == null)
            return;

        _metrics.HandleGroupMessage(message.Chat.Id, messageText.Length);
        if (!_validator.GroupMessageValid(message, messageText))
        {
            _logger.Information("Message by {ChatId} | {From} is not valid", message.Chat.Id, message.From);
            return;
        }

        if (await _groupsBlacklist.InBlacklist(message.Chat.Id))
            return;

        if (message.IsCommand() && !ManualTranslationCommands.Contains(message.Text))
        {
            _logger.Information("Message by {ChatId} | {From} is a command", message.Chat.Id, message.From);
            await _commandHandler.Handle(message);
            return;
        }

        if (messageText.StartsWith('!') && !ManualTranslationCommands.Contains(messageText))
            return;

        if (messageText.StartsWith($"@{Static.Username} set:", StringComparison.InvariantCulture))
        {
            _logger.Information("Message by {ChatId} | {From} is a setting changing", message.Chat.Id, message.From);
            await _settingsChangeHandler.Handle(message);
            return;
        }

        var groupSettings = await _settingsService.GetSettings(message.Chat.Id);
        switch (groupSettings.TranslationMode)
        {
            case TranslationMode.Forwards:
                _logger.Information("Group {ChatId} | {From} is using Forwards translation mode", message.Chat.Id, message.From);
                if ((message.ForwardFrom == null || message.ForwardFrom.Id == 1087968824)
                    && message.ForwardSenderName == null
                    && message.ForwardFromChat == null
                    && message.ForwardSignature == null
                    && message.ForwardDate == null
                    && message.ForwardFromMessageId == null)
                    return;

                break;
            case TranslationMode.Manual:
                _logger.Information("Group {ChatId} | {From} is using Manual translation mode", message.Chat.Id, message.From);
                if (message.ReplyToMessage == null || !ManualTranslationCommands.Contains(messageText))
                    return;

                messageText = message.ReplyToMessage.TextOrCaption();
                break;
            case TranslationMode.LinkedChannel:
                if (message.From is not { Id: 777000 })
                    return;

                break;
        }

        if (message.From != null)
            await _users.AddFromGroupIfNeeded(message.From.Id);

        await _translateHandler.Handle(message, messageText, groupSettings);
    }

    private async Task HandlePrivateMessage(Message message)
    {
        if (message.From == null)
            return;

        await _users.AddFromPmIfNeeded(message.From.Id, null);

        if (message.IsCommand())
        {
            await _commandHandler.Handle(message);
            return;
        }

        await _botMenu.SendStart(message.Chat.Id);
    }
}