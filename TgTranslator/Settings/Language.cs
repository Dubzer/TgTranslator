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

        protected override void GenerateButtons()
        {
            buttons.Add(new List<InlineKeyboardButton> { new InlineKeyboardButton { Text = "🇬🇧 English", CallbackData = $"switch: ApplyMenu {command}en" }, new InlineKeyboardButton { Text = "🇪🇸 Spanish", CallbackData = $"switch: ApplyMenu {command}es" }, new InlineKeyboardButton { Text = "🇷🇺 Russian", CallbackData = $"switch: ApplyMenu {command}ru" } });
            buttons.Add(new List<InlineKeyboardButton> { new InlineKeyboardButton { Text = "🇩🇪 German", CallbackData = $"switch: ApplyMenu {command}de" }, new InlineKeyboardButton { Text = "🇫🇷 French", CallbackData = $"switch: ApplyMenu {command}fr" }, new InlineKeyboardButton { Text = "🇸🇪 Swedish", CallbackData = $"switch: ApplyMenu {command}sv" } });
            buttons.Add(new List<InlineKeyboardButton> { new InlineKeyboardButton { Text = "🇨🇳 Chinese", CallbackData = $"switch: ApplyMenu {command}zh" }, new InlineKeyboardButton { Text = "🇮🇷 Persian", CallbackData = $"switch: ApplyMenu {command}fa" }, new InlineKeyboardButton { Text = "🇮🇳 Hindi", CallbackData = $"switch: ApplyMenu {command}hi" } });

            base.GenerateButtons();
        }
    }
}
