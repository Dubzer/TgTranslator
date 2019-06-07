using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    abstract class Setting
    {
        protected List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
        protected string message;

        protected virtual InlineKeyboardMarkup GenerateButtons()
        {
            buttons.Add(new InlineKeyboardButton { Text = "Back"});
            return new InlineKeyboardMarkup(buttons);
        }
    }
}
