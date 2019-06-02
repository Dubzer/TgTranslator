using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;
using YandexTranslateCSharpSdk;

namespace translathor
{
    class UpdateHandler
    {
        YandexTranslateSdk translate;

        public UpdateHandler()
        {
            translate = new YandexTranslateSdk();

            translate.ApiKey = Translathor.Configuration["tokens:yandex"];
        }
        public async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            LoggingService.Log($"Got: {e.Message.Text} by @{e.Message.From.Username} from {e.Message.Chat.Title}");

            if (!Blacklists.IsEnglish(e.Message.Text))
            {
                string translation;
                try
                {
                    translation = await translate.TranslateText(e.Message.Text, "en");
                }
                catch(Exception exception)
                {
                    LoggingService.Log("Got exception while tried to translate message: \n" + exception.ToString());
                    return;
                }
                
                LoggingService.Log($"Translated {e.Message.Text} to {translation}");
                try
                {
                    await Translathor.botClient.SendTextMessageAsync(e.Message.Chat.Id, translation, Telegram.Bot.Types.Enums.ParseMode.Default, true, true, e.Message.MessageId);
                }
                catch(Exception exception)
                {
                    LoggingService.Log("Got exception while tried to send message: \n" + exception.ToString());
                    return;
                }
            }
        }
    }
}
