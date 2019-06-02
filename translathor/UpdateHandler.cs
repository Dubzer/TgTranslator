using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;
using YandexTranslateCSharpSdk;

namespace translathor
{
    class UpdateHandler
    {
        YandexTranslateSdk wrapper;

        public UpdateHandler()
        {
            wrapper = new YandexTranslateSdk();

            wrapper.ApiKey = Translathor.Configuration["tokens:yandex"];
        }
        public async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + e.Message.Text);

            string language = await wrapper.DetectLanguage(e.Message.Text);
            if(Blacklists.Verify(language, Blacklists.languagesBlacklist))
            {
                string translation = await wrapper.TranslateText(e.Message.Text, "en");
                await Translathor.botClient.SendTextMessageAsync(e.Message.Chat.Id, translation, Telegram.Bot.Types.Enums.ParseMode.Default, true, true, e.Message.MessageId);
            }
        }
    }
}
