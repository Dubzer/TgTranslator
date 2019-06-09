using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    abstract class Setting
    {
        protected List<InlineKeyboardButton> buttons;
        public string description;
        public string itemTitle;
        public string command;

        public virtual void GenerateButtons()
        {
            buttons = new List<InlineKeyboardButton>
            {
                new InlineKeyboardButton { Text = "Back to the menu", CallbackData = "MainMenu"}
            };
        }

        public void GenerateApplyOrCancel()
        {
            buttons = new List<InlineKeyboardButton>
            {
                new InlineKeyboardButton { Text = "Apply", SwitchInlineQuery = command },
                new InlineKeyboardButton { Text = "Cancel", CallbackData = "MainMenu"}
            };
        }

        public virtual InlineKeyboardMarkup GenerateMarkup()
        {
            GenerateButtons();
            return new InlineKeyboardMarkup(buttons);
        }
    }
}
