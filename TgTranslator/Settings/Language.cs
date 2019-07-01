using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Settings
{
    class Language : Setting
    {
        public Language(string itemTitle)
        {
            this.itemTitle = itemTitle;
            command = "lang=";
            description = "Here you can setup primary language for your group. If you don't see your language, select **Other**";
        }

        protected override void GenerateButtons()
        {
            List<InlineKeyboardButton> tempKeyboard = new List<InlineKeyboardButton>();

            foreach (var language in Program.languages)
            {
                if (tempKeyboard.Count == 3)
                {
                    buttons.Add(tempKeyboard.ToList());
                    tempKeyboard.Clear();
                }
                
                tempKeyboard.Add(new InlineKeyboardButton
                            {
                                Text = $@"{language.Flag} {language.Name}",
                                CallbackData = $"switch: ApplyMenu {command}{language.Code}"
                            });
            }
            
            // Adds remaining languages
            if(tempKeyboard.Count != 0)
                buttons.Add(tempKeyboard);
            
            base.GenerateButtons();
        }
    }
}
