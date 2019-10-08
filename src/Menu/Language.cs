using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Menu
{
    class Language : MenuItem
    {
        public Language(string[] arguments = null) 
        {
            itemTitle = "Group main language";
            command = "lang";
            description = "Here you can setup primary language for your group:";
        }

        protected override void GenerateButtons()
        {
            List<InlineKeyboardButton> buffer = new List<InlineKeyboardButton>();

            foreach (var language in Program.languages)
            {
                // Adds new row of buttons that already created before
                if (buffer.Count == 3)
                {    
                    buttons.Add(buffer.ToList());
                    buffer.Clear();
                }
                
                buffer.Add(new InlineKeyboardButton
                            {
                                Text = $@"{language.Flag} {language.Name}",
                                CallbackData = $"switch {typeof(ApplyMenu)}#{command}={language.Code}"
                            });
            }
            
            // Adds remaining languages
            if (buffer.Count != 0)
            {
                buttons.Add(buffer.ToList());
                buffer.Clear();
            }
            
            base.GenerateButtons();
        }
    }
}
