using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    class Language : Setting
    {
        public Language(string itemTitle)
        {
            this.itemTitle = itemTitle;
            command = "switchlang";
            message = "Here you can setup primary language for your group";
        }

        public override InlineKeyboardMarkup GenerateButtons()
        {
            return base.GenerateButtons();
        }
    }
}
