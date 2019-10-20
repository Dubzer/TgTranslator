using System;
using System.Collections.Generic;

namespace TgTranslator.Menu
{
    public class MainMenu : MenuItem
    {
        private List<Type> _mainMenuItems;

        public MainMenu(IReadOnlyList<string> arguments)
        {
            description = "You must add this bot to your group. Then, you can configure it here:";
            itemTitle = "MainMenu";
            
            _mainMenuItems = new List<Type>
            {
                typeof(LanguageMenu),
                typeof(ModeMenu)
            };
        }

        protected override void GenerateButtons()
        {
            base.GenerateButtons(_mainMenuItems);
        }
    }
}