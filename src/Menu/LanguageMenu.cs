﻿using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using TgTranslator.Services;

namespace TgTranslator.Menu;

class LanguageMenu : MenuItem
{
    private const byte LangsOnPage = 30;
    private const byte Columns = 2;

    private readonly int _currentPage;
    private readonly int _pagesNeeded;

    public LanguageMenu(IReadOnlyList<string> arguments)
    {
        _currentPage = (arguments?.Count).GetValueOrDefault(0) > 0
            ? byte.Parse(arguments[0])
            : _currentPage = 1;

        _pagesNeeded = (int) Math.Ceiling(Static.Languages.Count / (double) LangsOnPage);

        ItemTitle = "Change group language";
        Command = Setting.Language.ToString().ToLowerInvariant();
        Description = $"*Here you can setup primary language for your group.*\n{Static.Username} won't translate messages written in this language";
    }

    protected override void GenerateButtons()
    {
        var buffer = new List<InlineKeyboardButton>();

        int previousPage = _currentPage - 1;
        int displayedLanguages = previousPage * LangsOnPage;
        int languagesLeft = Static.Languages.Count - displayedLanguages;

        int until = _currentPage == _pagesNeeded // Is this the last page?
            ? displayedLanguages + languagesLeft
            : displayedLanguages + LangsOnPage;

        for (int i = displayedLanguages; i < until; i++)
        {
            // Adds new row of buttons that already created before
            if (buffer.Count == Columns)
            {
                Buttons.Add(buffer.ToList());
                buffer.Clear();
            }

            var language = Static.Languages[i];
            buffer.Add(new InlineKeyboardButton($@"{language.Flag} {language.Name}")
            {
                CallbackData = $"switch {typeof(ApplyMenu)}#{Command}={language.Code}"
            });
        }

        // Adds remaining languages
        if (buffer.Count != 0)
        {
            Buttons.Add(buffer.ToList());
            buffer.Clear();
        }

        AddNavigateButtons();
    }

    private void AddNavigateButtons()
    {
        var controlButtons = new List<InlineKeyboardButton>();

        if (_currentPage > 1)
            controlButtons.Add(new InlineKeyboardButton( "⬅")
                {CallbackData = $"switch {typeof(LanguageMenu)}#{_currentPage - 1}"});

        controlButtons.Add(new InlineKeyboardButton("❌ Back") {CallbackData = $"switch {typeof(MainMenu)}"});

        if (_currentPage < _pagesNeeded)
            controlButtons.Add(new InlineKeyboardButton("➡")
                {CallbackData = $"switch {typeof(LanguageMenu)}#{_currentPage + 1}"});

        Buttons.Add(controlButtons);
    }
}