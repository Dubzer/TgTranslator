using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    class Language : Setting
    {
        public Language(string itemTitle)
        {
            this.itemTitle = itemTitle;
            command = "lang=";
            description = "Here you can setup primary language for your group. If you don't see your language, select **Other**";
        }

        public override void GenerateButtons()
        {
            buttons.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton { Text = "🇬🇧 English", CallbackData = $"ApplyMenu {command}en" }, new InlineKeyboardButton { Text = "🇪🇸 Spanish", CallbackData = $"ApplyMenu {command}es" }, new InlineKeyboardButton { Text = "🇷🇺 Russian", CallbackData = $"ApplyMenu {command}ru" } });
            buttons.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton { Text = "🇩🇪 German", CallbackData = $"ApplyMenu {command}de" }, new InlineKeyboardButton { Text = "🇫🇷 French", CallbackData = $"ApplyMenu {command}fr" }, new InlineKeyboardButton { Text = "🇸🇪 Swedish", CallbackData = $"ApplyMenu {command}sv" } });
            buttons.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton { Text = "🇨🇳 Chinese", CallbackData = $"ApplyMenu {command}zh" }, new InlineKeyboardButton { Text = "🇮🇷 Persian", CallbackData = $"ApplyMenu {command}fa" }, new InlineKeyboardButton { Text = "🇮🇳 Hindi", CallbackData = $"ApplyMenu {command}hi" } });

            base.GenerateButtons();
        }
    }
}
