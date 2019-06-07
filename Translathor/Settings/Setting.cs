using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace Translathor.Settings
{
    abstract class Setting
    {
        protected virtual InlineKeyboardMarkup GenerateButtons()
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            buttons.Add(new InlineKeyboardButton { })
            return new InlineKeyboardMarkup(buttons);
        }
    }
}
