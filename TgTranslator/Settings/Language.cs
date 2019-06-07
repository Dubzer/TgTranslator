using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    class Language : Setting
    {
        public Language()
        {
            message = "Here you can setup primary language for your group";
        }

        protected override InlineKeyboardMarkup GenerateButtons()
        {
            return base.GenerateButtons();
        }
    }
}
