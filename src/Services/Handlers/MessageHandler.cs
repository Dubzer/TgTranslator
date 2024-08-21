using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sentry;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TgTranslator.Data.Options;
using TgTranslator.Exceptions;
using TgTranslator.Interfaces;
using TgTranslator.Menu;
using TgTranslator.Stats;
using TgTranslator.Utils;
using TgTranslator.Utils.Extensions;
using TgTranslator.Validation;

namespace TgTranslator.Services.Handlers;

public class MessageHandler : IMessageHandler
{
    private readonly Blacklists _blacklist;
    private readonly BotMenu _botMenu;

    private readonly TelegramBotClient _client;
    private readonly Metrics _metrics;
    private readonly SettingsProcessor _settingsProcessor;
    private readonly CommandsManager _commandsManager;
    private readonly ITranslator _translator;
    private readonly MessageValidator _validator;
    private readonly UsersDatabaseService _users;
    private readonly GroupsBlacklistService _groupsBlacklist;
    private readonly ILogger _logger;

    private readonly HashSet<string> _manualTranslationCommands;

    public MessageHandler(TelegramBotClient client, BotMenu botMenu, SettingsProcessor settingsProcessor, CommandsManager commandsManager,
        ITranslator translator, Metrics metrics, IOptions<Blacklists> blacklistsOptions, MessageValidator validator, UsersDatabaseService users, GroupsBlacklistService groupsBlacklist, ILogger logger)
    {
        _client = client;
        _botMenu = botMenu;
        _settingsProcessor = settingsProcessor;
        _commandsManager = commandsManager;
        _translator = translator;
        _metrics = metrics;
        _blacklist = blacklistsOptions.Value;
        _validator = validator;
        _users = users;
        _groupsBlacklist = groupsBlacklist;
        _logger = logger;
        _manualTranslationCommands = [$"@{Static.Username}", "!translate", "/tl", $"/tl@{Static.Username}"];
    }

    #region IMessageHandler Members

    public async Task HandleMessageAsync(Message message)
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

        if (_blacklist.UserIdsBlacklist.Contains(message.From.Id))
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

    #endregion

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

        if (message.IsCommand() && !_manualTranslationCommands.Contains(messageText))
        {
            _logger.Information("Message by {ChatId} | {From} is a command", message.Chat.Id, message.From);
            await HandleCommand(message);
            return;
        }


        if(messageText.StartsWith('!') && !_manualTranslationCommands.Contains(messageText))
            return;

        if (messageText.StartsWith($"@{Static.Username} set:", StringComparison.InvariantCulture))
        {
            _logger.Information("Message by {ChatId} | {From} is a setting changing", message.Chat.Id, message.From);
            await HandleSettingChanging(message);
            return;
        }

        switch (await _settingsProcessor.GetTranslationMode(message.Chat.Id))
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
                if (message.ReplyToMessage == null)
                    return;
                if (_manualTranslationCommands.Contains(messageText))
                    await HandleTranslation(message.ReplyToMessage, message.ReplyToMessage.TextOrCaption());
                return;
            case TranslationMode.LinkedChannel:
                if (message.From is not { Id: 777000 })
                    return;
                break;
        }

        await HandleTranslation(message, messageText);
    }

    private async Task HandlePrivateMessage(Message message)
    {
        await _users.AddFromPmIfNeeded(message.From.Id, null);
        if (message.IsCommand())
        {
            await HandleCommand(message);
            return;
        }

        await _botMenu.SendStart(message.Chat.Id);
    }

    private async Task HandleTranslation(Message message, string text)
    {
        _logger.Information("Handling translation for {ChatId} | {From}...", message.Chat.Id, message.From);
        _metrics.HandleTranslatorApiCall(message.Chat.Id, string.IsNullOrEmpty(text) ? 0 : text.Length);
        await _users.AddFromGroupIfNeeded(message.From.Id);

        if (!text.AnyLetters())
            return;

        var groupConfig = await _settingsProcessor.GetGroupConfiguration(message.Chat.Id);

        if (groupConfig.Delay != 0 && groupConfig.TranslationMode == TranslationMode.Auto)
            await Task.Delay(TimeSpan.FromSeconds(groupConfig.Delay));

        var translation = await _translator.TranslateTextAsync(text, groupConfig.Languages[0]);
        var translatedText = translation.Text;

        if (translation.DetectedLanguage == groupConfig.Languages[0])
            return;

        var emojiRegex = new Regex(@"\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff]", RegexOptions.Compiled);
        var nonLettersRegex = new Regex(@"\p{P}|\d|\s", RegexOptions.Compiled);

        string normalizedText = nonLettersRegex.Replace(text, "");
        normalizedText = emojiRegex.Replace(normalizedText, "");

        string normalizedTranslation = nonLettersRegex.Replace(translatedText, "");
        normalizedTranslation = emojiRegex.Replace(normalizedTranslation, "");

        if (string.IsNullOrEmpty(normalizedTranslation) || string.Equals(normalizedText, normalizedTranslation, StringComparison.InvariantCultureIgnoreCase))
            return;

        translatedText = TranslationUtils.FixEntities(message.Text, translatedText, message.Entities);

        // In case of multiple translation messages, will contain the last one sent
        Message translationMessage;
        if (translatedText.Length <= 4096)
        {
            translationMessage = await _client.SendTextMessageAsync(message.Chat.Id, translatedText,
                disableNotification: true, linkPreviewOptions: TelegramUtils.DisabledLinkPreview,
                replyParameters: TelegramUtils.SafeReplyTo(message.MessageId));
        }
        else
        {
            translationMessage = await SendLongMessage(message.Chat.Id, translatedText, message.MessageId);
        }


        // The time between the original message and the translation message
        var translationMs = ((DateTimeOffset)translationMessage.Date).ToUnixTimeMilliseconds() -
                            ((DateTimeOffset)message.Date).ToUnixTimeMilliseconds();

        _metrics.TranslationResponseTime.Observe(translationMs);
        if (translationMs > 10000)
            _logger.Warning("Abnormal translation time for {ChatId} | {From} | {Time}ms",
                message.Chat.Id,
                message.From,
                translationMs);

        _logger.Information("Sent translation to {ChatId} | {From}", message.Chat.Id, message.From);
    }

    private async Task<Message> SendLongMessage(long chatId, string message, int replyId)
    {
        var (firstPart, secondPart) = MessageSplitter.Split(message);

        var firstPartResult = await _client.SendTextMessageAsync(chatId, firstPart,
            disableNotification: true, linkPreviewOptions: TelegramUtils.DisabledLinkPreview,
            replyParameters: TelegramUtils.SafeReplyTo(replyId));

        return await _client.SendTextMessageAsync(chatId, secondPart,
            disableNotification: true, linkPreviewOptions: TelegramUtils.DisabledLinkPreview,
            replyParameters: TelegramUtils.SafeReplyTo(firstPartResult.MessageId));
    }

    private async Task HandleSettingChanging(Message message)
    {
        if (!await message.From.IsAdministrator(message.Chat.Id, _client))
            throw new UnauthorizedSettingChangingException();

        string tinyString = message.Text.Replace($"@{Static.Username} set:", "");

        (string param, string value) = (tinyString.Split('=')[0], tinyString.Split('=')[1]);

        if (!Enum.TryParse(param, true, out Setting setting) || !Enum.IsDefined(typeof(Setting), setting))
            throw new InvalidSettingException();

        if (!_settingsProcessor.ValidateSettings(Enum.Parse<Setting>(param, true), value))
            throw new InvalidSettingValueException();

        switch (setting)
        {
            case Setting.Language:
                await _settingsProcessor.ChangeLanguage(message.Chat.Id, value);
                break;
            case Setting.Mode:
                var mode = Enum.Parse<TranslationMode>(value, true);

                await _commandsManager.ChangeGroupMode(message.Chat.Id, mode);
                await _settingsProcessor.ChangeMode(message.Chat.Id, mode);

                break;
        }

        await _client.SendTextMessageAsync(
            message.Chat.Id,
            "Done!",
            replyParameters: new ReplyParameters
            {
                MessageId = message.MessageId,
                AllowSendingWithoutReply = false,
            });
    }

    private async Task HandleCommand(Message message)
    {
        ChatType chatType = message.Chat.Type;
        string command = message.Text[1..];
        string payload = null;

        if (command.Contains('@'))
        {
            int indexOfAt = command.IndexOf('@');
            if(command[(indexOfAt + 1)..] != Static.Username)
                return;

            command = command[..indexOfAt];
        }

        if (command.Contains(' '))
        {
            payload = command.Split(" ")[1];
            command = command.Split(" ")[0];
        }

        switch (command)
        {
            case "settings" when chatType is ChatType.Group or ChatType.Supergroup:
                if(!await message.From.IsAdministrator(message.Chat.Id, _client))
                    throw new UnauthorizedSettingChangingException();

                var bot = await _client.GetChatMemberAsync(message.Chat.Id, Static.BotId);
                if (message.From?.Id == 1087968824 && bot.Status != ChatMemberStatus.Administrator)
                {
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        $"⚠️ To change the settings, you need to promote @{Static.Username} to administrator status!");

                    return;
                }

                await _client.SendTextMessageAsync(message.Chat.Id,
                    "Press on the button bellow to change the settings." +
                    $"\n\nIf your client doesn't support the menu [click here](https://t.me/{Static.Username}?start=s)",
                    parseMode: ParseMode.Markdown,
                    linkPreviewOptions: TelegramUtils.DisabledLinkPreview,
                    replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Change settings")
                    {
                        Url = $"https://t.me/{Static.Username}/settings?startapp=i{message.Chat.Id}"
                    }));
                break;
            case "settings" when chatType == ChatType.Private:
                await _client.SendTextMessageAsync(message.Chat.Id,
                    "You cannot configure the bot here 😳\nPlease use this command in the group.");
                break;
            case "start" when chatType == ChatType.Private && payload == "s":
                await _botMenu.SendSettings(message.Chat.Id);
                break;
            case "start" when chatType == ChatType.Private:
                if (!string.IsNullOrEmpty(payload))
                    await _users.AddFromPmIfNeeded(message.From.Id, payload);

                await _botMenu.SendStart(message.Chat.Id);
                break;
            case "contact" when chatType == ChatType.Private:
                await _client.SendTextMessageAsync(message.Chat.Id, "Developer: @Dubzer\nNews channel: @tgtrns\n\n☕️ Donate: yaso.su/feedme");
                break;
            default:
                return;
        }
    }
}