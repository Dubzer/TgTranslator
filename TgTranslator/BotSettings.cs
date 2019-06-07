using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;
using TgTranslator.Settings;

namespace TgTranslator
{
    class BotSettings
    {
        List<Setting> settings;

        public BotSettings()
        {
            settings = new List<Setting>()
            {
                new Language("Language")
            };
        }

        void SwitchItem(MessageEventArgs e, Setting item)
        {
            Program.botClient.EditMessageCaptionAsync(e.Message.Chat.Id, e.Message.From.Id, item.message, item.GenerateButtons());
        }
    }
}
