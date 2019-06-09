using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    class ApplyMenu : Setting
    {
        public ApplyMenu(string command)
        {
            description = "Everything is done. To apply settings, click on *Apply* button, or if you don't want to apply, click on **Cancel**";
            this.command = command;
        }

        public override void GenerateButtons()
        {
            buttons.Add(new List<InlineKeyboardButton>()
                        {
                            new InlineKeyboardButton { Text = "Apply", SwitchInlineQuery = command },
                            new InlineKeyboardButton { Text = "Cancel", CallbackData = "MainMenu"}
                        });
        }
    }
}
