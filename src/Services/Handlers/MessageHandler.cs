using System;
using System.Diagnostics;
using Serilog;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Exceptions;
using TgTranslator.Extensions;
using TgTranslator.Menu;
using TgTranslator.Stats;
using TgTranslator.Translation;

namespace TgTranslator.Services.Handlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly TelegramBotClient _client;
        private readonly BotMenu _botMenu;
        private readonly SettingsProcessor _settingsProcessor;
        private readonly ITranslator _translator;
        private readonly IMetrics _metrics;
        private readonly ILanguageDetector _languageDetector;
        private readonly Blacklist _blacklist;
        private readonly MessageValidator _validator;
        
        private readonly string _botUsername;
        
        public MessageHandler(TelegramBotClient client, BotMenu botMenu, SettingsProcessor settingsProcessor,
            ILanguageDetector languageLanguageDetector, ITranslator translator, IMetrics metrics, Blacklist blacklist, MessageValidator validator)
        {
            _client = client;
            _botMenu = botMenu;
            _settingsProcessor = settingsProcessor;
            _translator = translator;
            _metrics = metrics;
            _languageDetector = languageLanguageDetector;
            _blacklist = blacklist;
            _validator = validator;
            
            _botUsername = _client.GetMeAsync().Result.Username;
        }

        public async Task HandleMessageAsync(Message message)
        {
            _metrics.HandleMessage();
            if (_blacklist.IsUserBlocked(message.From.Id))
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
            if (!_validator.GroupMessageValid(message))
                return;
            
            _metrics.HandleGroupMessage(message.Chat.Id, message.Text.Length);
            
            if (message.Text.Contains($"{_botUsername} set:"))
            {
                await HandleSettingChanging(message);
                return;
            }

            switch (await _settingsProcessor.GetTranslationMode(message.Chat.Id))
            {
                case TranslationMode.Forwards:
                    if(message.ForwardSenderName == null)
                        return;
                    break;
                case TranslationMode.Manual:
                    if (message.ReplyToMessage == null) return;
                    if(message.Text == _botUsername || message.Text == "!translate" || await RequireTranslation(message.Text, await _settingsProcessor.GetGroupLanguage(message.Chat.Id)))
                        await HandleTranslation(message.ReplyToMessage);
                    
                    return;
            }
            
            if (await RequireTranslation(message.Text, await _settingsProcessor.GetGroupLanguage(message.Chat.Id)))
            {
                await HandleTranslation(message);
            }
        }

        private async Task HandlePrivateMessage(Message message)
        {
            Log.Information($"Got: [ {message.Text} ] by {message.From.Username} from PM CHAT");
            await _botMenu.SendMainMenu(message.Chat.Id);
        }

        private async Task HandleTranslation(Message message)
        {
            _metrics.HandleTranslatorApiCall(message.Chat.Id, message.Text.Length);
            string translation = await _translator.TranslateTextAsync(message.Text, "",
                await _settingsProcessor.GetGroupLanguage(message.Chat.Id));

            if (string.IsNullOrEmpty(translation) || string.Equals(message.Text, translation, StringComparison.CurrentCultureIgnoreCase))
                return;

            await _client.SendTextMessageAsync(message.Chat.Id, translation, ParseMode.Default, true, true,
                message.MessageId);

            Log.Information($"Sent translation to {message.Chat.Id} | {message.From.Id}");
        }

        private async Task HandleSettingChanging(Message message)
        {    
            if (!await message.From.IsAdministrator(message.Chat.Id, _client))
                throw new UnauthorizedSettingChangingException();

            string tinyString = message.Text.Replace($"@{_botUsername} set:", "");

            (string param, string value) = (tinyString.Split('=')[0], tinyString.Split('=')[1]);

            if (!Enum.TryParse(param, true, out Setting setting) || !Enum.IsDefined(typeof(Setting), setting))
                throw new InvalidSettingException();
            
            if (!_settingsProcessor.ValidateSettings(Enum.Parse<Setting>(value, true), value))
                throw new InvalidSettingValueException();

            switch (setting)
            {
                case Setting.Language:
                    await _settingsProcessor.ChangeLanguage(message.Chat.Id, value);
                    break;
                case Setting.Mode:
                    await _settingsProcessor.ChangeMode(message.Chat.Id, Enum.Parse<TranslationMode>(value, true));
                    break;
            }
            
            await _client.SendTextMessageAsync(message.Chat.Id, "Done!", replyToMessageId: message.MessageId);
        }

        private async Task<bool> RequireTranslation(string text, string mainLanguage)
        {
            #region Check if string contains only non-alphabetic chars
            
            Regex regex = new Regex(@"\P{L}{1,}");
            var matches = regex.Matches(text);
            
            string match = matches.Count == 1 ? matches[0].Value : null;

            if (match != null && match.Length == text.Length)
                return false;

            #endregion
            
            string language = await _languageDetector.DetectLanguageAsync(text);

            return language != mainLanguage;
        }
    }
}