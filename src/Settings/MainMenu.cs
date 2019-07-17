using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    class MainMenu : Setting
    {
        
        public MainMenu(string itemTitle)
        {
            this.itemTitle = itemTitle;
            description = "There are the available settings:";
        }

        private void GenerateButtons(IEnumerable<Setting> settingsList)
        {
            foreach (var setting in settingsList)
            {
                if (setting.GetType().ToString() == "TgTranslator.Settings.MainMenu")
                    continue;

                buttons.Add(new List<InlineKeyboardButton> { new InlineKeyboardButton { Text = setting.itemTitle, CallbackData = $"switch: {setting.GetType()}" } });
            }
        }

        public InlineKeyboardMarkup GenerateMarkup(IEnumerable<Setting> settingsList)
        {
            if (buttons.Count == 0)
                GenerateButtons(settingsList);
            
            return GenerateMarkup();
        }
    }
}
    