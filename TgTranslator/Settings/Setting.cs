using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    abstract class Setting
    {
        protected List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
        public string message;
        public string itemTitle;

        public Setting()
        {
            this.itemTitle = itemTitle;
        }
        public virtual InlineKeyboardMarkup GenerateButtons()
        {
            buttons.Add(new InlineKeyboardButton { Text = "Back"});
            return new InlineKeyboardMarkup(buttons);
        }
    }
}
