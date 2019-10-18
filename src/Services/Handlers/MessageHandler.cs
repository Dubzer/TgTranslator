using Serilog;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Extensions;
using TgTranslator.Menu;
using TgTranslator.Translation;

namespace TgTranslator.Services.Handlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly TelegramBotClient _client;
        private readonly BotMenu _botMenu;
        private readonly SettingsProcessor _settingsProcessor;
        private readonly ITranslator _translator;
        private readonly ILanguageDetector _languageDetector;
        private readonly Blacklist _blacklist;
        private readonly MessageValidator _validator;
        
        private readonly string _botUsername;
        
        public MessageHandler(TelegramBotClient client, BotMenu botMenu, SettingsProcessor settingsProcessor,
            ILanguageDetector languageLanguageDetector, ITranslator translator, Blacklist blacklist, MessageValidator validator)
        {
            _client = client;
            _botMenu = botMenu;
            _settingsProcessor = settingsProcessor;
            _translator = translator;
            _languageDetector = languageLanguageDetector;
            _blacklist = blacklist;
            _validator = validator;
            
            _botUsername = _client.GetMeAsync().Result.Username;
        }

        public async Task HandleMessageAsync(Message message)
        {
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
            
            if (message.IsCommand())
            {
                if (message.Text == "/settings@" + _botUsername)
                    await _client.SendTextMessageAsync(message.Chat.Id, "Write me in PM and I will help you!",
                        ParseMode.Default, replyToMessageId: message.MessageId);

                return;
            }

            if (message.Text.Contains($"{_botUsername} set:lang"))
            {
                await HandleLanguageChanging(message);
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

            switch (message.Text)
            {
                case "/start":
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        "You should to add this bot in a group. Then, you can configure it in this chat by typing /settings");
                    break;
                case "/settings":
                    await _botMenu.SendSettingsMenu(message.Chat.Id);
                    break;
                default:
                    await _client.SendTextMessageAsync(message.Chat.Id, "Unknown command. Type /start to get help.");
                    break;
            }
        }

        private async Task HandleTranslation(Message message)
        {
            string translation = await _translator.TranslateTextAsync(message.Text, "",
                await _settingsProcessor.GetGroupLanguage(message.Chat.Id));

            if (message.Text == translation)
                return;

            await _client.SendTextMessageAsync(message.Chat.Id, translation, ParseMode.Default, true, true,
                message.MessageId);

            Log.Information($"Sent translation to {message.Chat.Id} | {message.From.Id}");
        }

        private async Task HandleLanguageChanging(Message message)
        {
            if (!await message.From.IsAdministrator(message.Chat.Id, _client))
            {
                await _client.SendTextMessageAsync(message.Chat.Id, "Hey! Only admins can change main language of this bot!",
                    ParseMode.Default, replyToMessageId: message.MessageId);
                
                return;
            }

            string tinyString = message.Text.Replace($"@{_botUsername} set:", "");

            string value = tinyString.Split('=')[1];

            if (!_settingsProcessor.ValidateLanguage(value))
            {
                await _client.SendTextMessageAsync(message.Chat.Id, "It seems that this language is not supported",
                    ParseMode.Default, false, true, message.MessageId);
                
                return;
            }

            await _settingsProcessor.ChangeLanguage(message.Chat.Id, value);
            await _client.SendTextMessageAsync(message.Chat.Id, "Done!", ParseMode.Default, false, true,
                message.MessageId);
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