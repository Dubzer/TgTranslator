using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using TgTranslator.Interfaces;

namespace TgTranslator.Settings
{
    class ApplyMenu : Setting, ITakesArguments
    {
        public ApplyMenu(string command)
        {
            description = "Everything is done. To apply settings, click on *Apply* button, or if you don't want to apply, click on **Cancel**";
            this.command = $"set:{command}";
        }

        protected override void GenerateButtons()
        {
            buttons.Add(new List<InlineKeyboardButton>
                        {
                            new InlineKeyboardButton { Text = "Apply", SwitchInlineQuery = command },
                            new InlineKeyboardButton { Text = "Cancel", CallbackData = "switch: MainMenu"}
                        });
        }
    }
}
