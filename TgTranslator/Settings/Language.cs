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
            command = "/switchlang ";
            description = "Here you can setup primary language for your group. If you don't see your language, select **Other**";
        }
        public override void GenerateButtons()
        {
            base.GenerateButtons();
        }

        public override InlineKeyboardMarkup GenerateMarkup()
        {
            GenerateButtons();
            return base.GenerateMarkup();
        }
    }
}
