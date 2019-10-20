using System;
using System.Collections.Generic;

namespace TgTranslator.Menu
{
    public class MainMenu : MenuItem
    {
        private List<Type> _mainMenuItems;

        public MainMenu(IReadOnlyList<string> arguments)
        {
            description = "Choose what you want:";
            itemTitle = "MainMenu";
            
            _mainMenuItems = new List<Type>
            {
                typeof(Language)
            };
        }

        protected override void GenerateButtons()
        {
            base.GenerateButtons(_mainMenuItems);
        }
    }
}