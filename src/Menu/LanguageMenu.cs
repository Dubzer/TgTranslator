using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Menu
{
    class LanguageMenu : MenuItem
    {
        private const byte LangsOnPage = 31;
        private const byte Columns = 2;
        private readonly int PagesNeeded;

        private readonly int _currentPage;

        public LanguageMenu(IReadOnlyList<string> arguments)
        {
            _currentPage = (arguments?.Count).GetValueOrDefault(0) > 0
                    ? byte.Parse(arguments[0])
                    : _currentPage = 1;

            PagesNeeded = (int)Math.Ceiling(Program.languages.Count / (double) LangsOnPage);
            
            itemTitle = "Change group language";
            command = "lang";
            description = "Here you can setup primary language for your group:";
        }

        protected override void GenerateButtons()
        {
            List<InlineKeyboardButton> buffer = new List<InlineKeyboardButton>();

            int previousPage = _currentPage - 1;
            int nextPage = _currentPage + 1;
            int displayedLanguages = previousPage * LangsOnPage;
            int languagesLeft = Program.languages.Count - displayedLanguages;

            int until = _currentPage == PagesNeeded // Is this the last page?
                ? displayedLanguages + languagesLeft
                : LangsOnPage * nextPage; 
            
            for (int i = displayedLanguages; i < until; i++)
            {
                // Adds new row of buttons that already created before
                if (buffer.Count == Columns)
                {    
                    buttons.Add(buffer.ToList());
                    buffer.Clear();
                }

                var language = Program.languages[i];
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
            
            AddNavigateButtons();
        }

        private void AddNavigateButtons()
        {
            List<InlineKeyboardButton> controlButtons = new List<InlineKeyboardButton>();
            
            if(_currentPage > 1)
                controlButtons.Add(new InlineKeyboardButton { Text = "⬅", CallbackData = $"switch {typeof(LanguageMenu)}#{_currentPage - 1}"});
            
            controlButtons.Add(new InlineKeyboardButton { Text = "❌ Back", CallbackData = $"switch {typeof(MainMenu)}"}); 

            if(_currentPage < PagesNeeded)
                controlButtons.Add(new InlineKeyboardButton { Text = "➡", CallbackData = $"switch {typeof(LanguageMenu)}#{_currentPage + 1}"});
            
            buttons.Add(controlButtons);
        }
    }
}
