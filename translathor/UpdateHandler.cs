using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using YandexTranslateCSharpSdk;
using Extensions;

namespace Translathor
{
    class UpdateHandler
    {
        YandexTranslateSdk translate;
        
        public UpdateHandler()
        {
            translate = new YandexTranslateSdk();

            translate.ApiKey = Program.Configuration["tokens:yandex"];
        }
        public async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            LoggingService.Log($"Got: {e.Message.Text} by {e.Message.From.Username} from {e.Message.Chat.Id}");
            string language;

            try
            {
                language = await DetectLanguage(e.Message.Text);
            }
            catch (Exception exception)
            {
                LoggingService.Log("Got exception while tried to detect language: \n" + exception.ToString());
                return;
            }

            if (Blacklists.Verify(language, Blacklists.languagesBlacklist))
            {
                string translation;
                try
                {
                    translation = await translate.TranslateText(e.Message.Text, $"{language}-en");
                }
                catch (Exception exception)
                {
                    LoggingService.Log("Got exception while tried to translate message: \n" + exception.ToString());
                    return;
                }

                LoggingService.Log($"Translated {e.Message.Text} ({language}) to {translation}");
                try
                {
                    await Program.botClient.SendTextMessageAsync(e.Message.Chat.Id, translation, Telegram.Bot.Types.Enums.ParseMode.Default, true, true, e.Message.MessageId);
                }
                catch (Exception exception)
                {
                    LoggingService.Log("Got exception while tried to send message: \n" + exception.ToString());
                    return;
                }
            }
        }

        public async Task<string> DetectLanguage(string text)
        {
            // Text without English symbols
            string textWOEng = Regex.Replace(text.WithoutLinks(), @"[A-Za-z0-9 .,-=@+(){}\[\]\\]", "");

            if (!string.IsNullOrWhiteSpace(textWOEng))
            {
                return await translate.DetectLanguage(textWOEng);
            }

            return await translate.DetectLanguage(text.WithoutLinks());
        }
    }
}
