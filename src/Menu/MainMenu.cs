using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TgTranslator.Menu;

public class MainMenu : MenuItem
{
    private readonly List<Type> _mainMenuItems;

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public MainMenu(IReadOnlyList<string> arguments)
    {
        Description =
            "This bot translates messages in the group chats.\nWrite /help to get started.\n\n⚙️ Then you can tweak some settings:";
        ItemTitle = "MainMenu";

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