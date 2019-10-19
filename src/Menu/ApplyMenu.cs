using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Menu
{
    class ApplyMenu : MenuItem
    {
        public ApplyMenu(string[] arguments)
        {
            description = "Everything is done. To apply settings, click on *Apply* button and choose your chat, otherwise click on *Cancel*";
            command = $"set:{arguments[0]}";
        }

        protected override void GenerateButtons()
        {
            buttons.Add(new List<InlineKeyboardButton>
                        {
                            new InlineKeyboardButton { Text = "✅ Apply", SwitchInlineQuery = command },
                            new InlineKeyboardButton { Text = "❌ Cancel", CallbackData = "switch " + typeof(MainMenu)}
                        });
        }
    }
}
    