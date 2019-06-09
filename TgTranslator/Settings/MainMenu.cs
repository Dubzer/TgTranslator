using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    class MainMenu : Setting
    {
        public MainMenu(string itemTitle)
        {
            this.itemTitle = itemTitle;
            description = "Some text that says something about settings";
        }

        public override void GenerateButtons()
        {
            buttons = new List<InlineKeyboardButton>();

            foreach (var setting in BotSettings.settings)
            {
                if (setting.GetType().ToString() == "TgTranslator.Settings.MainMenu")
                    continue;

                buttons.Add(new InlineKeyboardButton { Text = setting.itemTitle, CallbackData = setting.GetType().ToString() });
            }

        }
    }
}
