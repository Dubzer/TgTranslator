using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    class Language : Setting
    {
        public Language(string itemTitle)
        {
            this.itemTitle = itemTitle;
            command = "/switchlang=";
            description = "Here you can setup primary language for your group. If you don't see your language, select **Other**";
        }

        public override void GenerateButtons()
        {
            //buttons.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton { Text = "🇬🇧 English", CallbackData = $"ApplyMenu {command}en" }, new InlineKeyboardButton { Text = "🇪🇸 Spanish", CallbackData = $"ApplyMenu {command}es" } });
            //buttons.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton { Text = "🇮🇷 Persian", CallbackData = $"ApplyMenu {command}fa" }, new InlineKeyboardButton { Text = "🇷🇺 Russian", CallbackData = $"ApplyMenu {command}ru" } });
            //buttons.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton { Text = "🇮🇷 Persian", CallbackData = $"ApplyMenu {command}fa" }, new InlineKeyboardButton { Text = "🇷🇺 Russian", CallbackData = $"ApplyMenu {command}ru" } });
            buttons.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton { Text = "🇮🇷 Persian", CallbackData = $"ApplyMenu {command}fa" }, new InlineKeyboardButton { Text = "🇷🇺 Russian", CallbackData = $"ApplyMenu {command}ru" } , new InlineKeyboardButton { Text = "🇷🇺 Russian", CallbackData = $"ApplyMenu {command}ru" } });

            base.GenerateButtons();
        }
    }
}
