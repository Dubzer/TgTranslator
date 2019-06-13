using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{ 
    abstract class Setting
    {
        protected List<IEnumerable<InlineKeyboardButton>> buttons = new List<IEnumerable<InlineKeyboardButton>>();
        public string description;
        public string itemTitle;
        protected string command;

        protected virtual void GenerateButtons()
        {
            buttons.Add(new List<InlineKeyboardButton>
                        {
                            new InlineKeyboardButton { Text = "Back to the menu", CallbackData = "MainMenu"}
                        });
        }

        public InlineKeyboardMarkup GenerateMarkup()
        {
            if (buttons.Count == 0)
                GenerateButtons();

            return new InlineKeyboardMarkup(buttons);
        }
    }
}
