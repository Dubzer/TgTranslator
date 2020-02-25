using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TgTranslator.Menu
{
    public class MainMenu : MenuItem
    {
        private readonly List<Type> _mainMenuItems;

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        public MainMenu(IReadOnlyList<string> arguments)
        {
            Description =
                "You must add this bot to your group. Type /help to learn how. \nThen, you can configure it here:";
            ItemTitle = "MainMenu";

            _mainMenuItems = new List<Type>
            {
                typeof(LanguageMenu),
                typeof(ModeMenu)
            };
        }

        protected override void GenerateButtons() => base.GenerateButtons(_mainMenuItems);
    }
}