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
    private readonly ILanguageDetector _languageDetector;
    private readonly Metrics _metrics;
    private readonly SettingsProcessor _settingsProcessor;
    private readonly ITranslator _translator;
    private readonly MessageValidator _validator;
    private readonly UsersDatabaseService _users;
    private readonly GroupsBlacklistService _groupsBlacklist;
    private readonly ILogger _logger;

    private readonly HashSet<string> _manualTranslationCommands;
        
    public MessageHandler(TelegramBotClient client, BotMenu botMenu, SettingsProcessor settingsProcessor, ILanguageDetector languageLanguageDetector, 
        ITranslator translator, Metrics metrics, IOptions<Blacklists> blacklistsOptions, MessageValidator validator, UsersDatabaseService users, GroupsBlacklistService groupsBlacklist, ILogger logger)
    {
        _client = client;
        _botMenu = botMenu;
        _settingsProcessor = settingsProcessor;
        _translator = translator;
        _metrics = metrics;
        _languageDetector = languageLanguageDetector;
        _blacklist = blacklistsOptions.Value;
        _validator = validator;
        _users = users;
        _groupsBlacklist = groupsBlacklist;
        _logger = logger;
        _manualTranslationCommands = new HashSet<string> {$"@{Program.Username}", "!translate", "/tl", $"/tl@{Program.Username}"};
    }

    #region IMessageHandler Members

    public async Task HandleMessageAsync(Message message)
    {
        _metrics.HandleMessage();
        _logger.Information("Got new message {ChatId} | {From} | {ChatType}", message.Chat.Id, message.From, message.Chat.Type);
        SentrySdk.ConfigureScope(scope =>
        {
            scope.User = new Sentry.User
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
            
        if (messageText.StartsWith($"@{Program.Username} set:", StringComparison.InvariantCulture))
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
                if (_manualTranslationCommands.Contains(messageText) && await RequireTranslation(message.ReplyToMessage.TextOrCaption(), await _settingsProcessor.GetGroupLanguage(message.Chat.Id)))
                {
                    await HandleTranslation(message.ReplyToMessage, message.ReplyToMessage.TextOrCaption());
                }

                return;
            case TranslationMode.LinkedChannel:
                if (message.From is not { Id: 777000 })
                    return;
                break;
        }

        if (await RequireTranslation(messageText, await _settingsProcessor.GetGroupLanguage(message.Chat.Id)))
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

        await _botMenu.SendMainMenu(message.Chat.Id);
    }

    private async Task HandleTranslation(Message message, string text)
    {
        _logger.Information("Handling translation for {ChatId} | {From}...", message.Chat.Id, message.From);
        _metrics.HandleTranslatorApiCall(message.Chat.Id, string.IsNullOrEmpty(text) ? 0 : text.Length);
        await _users.AddFromGroupIfNeeded(message.From.Id);
            
        string groupLanguage = await _settingsProcessor.GetGroupLanguage(message.Chat.Id);
        string translation = await _translator.TranslateTextAsync(text, groupLanguage);

        var emojiRegex = new Regex(@"\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff]", RegexOptions.Compiled);
        var nonLettersRegex = new Regex(@"\p{P}|\d|\s", RegexOptions.Compiled);
        
        string normalizedText = nonLettersRegex.Replace(text, "");
        normalizedText = emojiRegex.Replace(normalizedText, "");
        
        string normalizedTranslation = nonLettersRegex.Replace(translation, "");
        normalizedTranslation = emojiRegex.Replace(normalizedTranslation, "");
        
        if (string.IsNullOrEmpty(normalizedTranslation) || string.Equals(normalizedText, normalizedTranslation, StringComparison.InvariantCultureIgnoreCase))
            return;

        translation = TranslationUtils.FixEntities(message.Text, translation, message.Entities);

        if (translation.Length <= 4096)
        {
            await _client.SendTextMessageAsync(message.Chat.Id, translation,
                disableNotification: true, disableWebPagePreview: true,
                replyToMessageId: message.MessageId, allowSendingWithoutReply: false);
        }
        else
        {
            await SendLongMessage(message.Chat.Id, translation, message.MessageId);
        }

        _logger.Information("Sent translation to {ChatId} | {From}", message.Chat.Id, message.From);
    }

    private async Task SendLongMessage(long chatId, string message, int replyId)
    {
        var (firstPart, secondPart) = MessageSplitter.Split(message);

        var firstPartResult = await _client.SendTextMessageAsync(chatId, firstPart,
            disableNotification: true, disableWebPagePreview: true,
            replyToMessageId: replyId, allowSendingWithoutReply: false);

        await _client.SendTextMessageAsync(chatId, secondPart,
            disableNotification: true, disableWebPagePreview: true,
            replyToMessageId: firstPartResult.MessageId, allowSendingWithoutReply: false);
    }

    private async Task HandleSettingChanging(Message message)
    {
        if (!await message.From.IsAdministrator(message.Chat.Id, _client))
            throw new UnauthorizedSettingChangingException();

        string tinyString = message.Text.Replace($"@{Program.Username} set:", "");

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
                
                await _client.DeleteMyCommandsAsync(BotCommandScope.Chat(message.Chat.Id));
                
                switch (mode)
                {
                    case TranslationMode.Manual:
                        await _client.SetMyCommandsAsync(new[]
                        {
                            BotCommands.AdminCommand,
                            BotCommands.TranslateCommand
                        }, BotCommandScope.ChatAdministrators(message.Chat.Id));

                        await _client.SetMyCommandsAsync(new[]
                        {
                            BotCommands.TranslateCommand
                        }, BotCommandScope.Chat(message.Chat.Id));
                        break;
                    default:
                        await _client.DeleteMyCommandsAsync(BotCommandScope.Chat(message.Chat.Id));
                        await _client.SetMyCommandsAsync(new[]
                        {
                            BotCommands.AdminCommand
                        }, BotCommandScope.ChatAdministrators(message.Chat.Id));
                        break;
                }

                
                await _settingsProcessor.ChangeMode(message.Chat.Id, mode);
                break;
        }

        await _client.SendTextMessageAsync(message.Chat.Id, "Done!", replyToMessageId: message.MessageId, allowSendingWithoutReply: false);
    }

    private async Task HandleCommand(Message message)
    {
        ChatType chatType = message.Chat.Type;
        string command = message.Text[1..];
        string payload = null;
            
        if (command.Contains('@'))
        {
            int indexOfAt = command.IndexOf('@');
            if(command.Substring(indexOfAt + 1) != Program.Username)
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
                    
                const string text = "You can access settings in the PM";
                await _client.SendTextMessageAsync(message.Chat.Id, text, replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Open settings")
                {
                    Url = $"https://t.me/{Program.Username}?start=s"
                }));
                    
                break;
            case "start" when chatType == ChatType.Private && !string.IsNullOrEmpty(payload):
                if (payload != "s")
                    await _users.AddFromPmIfNeeded(message.From.Id, payload);

                await _botMenu.SendMainMenu(message.Chat.Id);
                break;
            case "start" when chatType == ChatType.Private:
            case "settings" when chatType == ChatType.Private:
                await _botMenu.SendMainMenu(message.Chat.Id);
                break;
            case "help" when chatType == ChatType.Private:
                await _client.SendVideoAsync(message.Chat.Id, new InputFileUrl("https://dubzer.dev/TgTranslatorHelp.mp4"));
                break;
            case "contact" when chatType == ChatType.Private:
                await _client.SendTextMessageAsync(message.Chat.Id, "Developer: @Dubzer\nNews channel: @tgtrns\n\n☕️ Donate: yaso.su/feedme");
                break;
            default:
                return;
        }
    }

    private async Task<bool> RequireTranslation(string text, string mainLanguage)
    {
        #region Check if string contains only non-alphabetic chars

        var regex = new Regex(@"\P{L}{1,}");
        var matches = regex.Matches(text);

        string match = matches.Count == 1 ? matches[0].Value : null;

        if (match != null && match.Length == text.Length)
            return false;

        #endregion

        string language = await _languageDetector.DetectLanguageAsync(text);

        return language != mainLanguage;
    }
}