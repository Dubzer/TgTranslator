using System.Collections.Generic;
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

        private void GenerateButtons(IEnumerable<Setting> settingsList)
        {
            foreach (var setting in settingsList)
            {
                if (setting.GetType().ToString() == "TgTranslator.Settings.MainMenu")
                    continue;

                buttons.Add(new List<InlineKeyboardButton> { new InlineKeyboardButton { Text = setting.itemTitle, CallbackData = setting.GetType().ToString() } });
            }
        }

        public InlineKeyboardMarkup GenerateMarkup(IEnumerable<Setting> settingsList)
        {
            GenerateButtons(settingsList);
            return GenerateMarkup();
        }
    }
}
