using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Menu;

public abstract class MenuItem
{
    protected readonly List<IEnumerable<InlineKeyboardButton>> Buttons = new List<IEnumerable<InlineKeyboardButton>>();
    protected string Command;
    public string Description;
    protected string ItemTitle;

    /// <summary>
    /// Generates default buttons
    /// </summary>
    protected virtual void GenerateButtons()
    {
        Buttons.Add(new List<InlineKeyboardButton>
        {
            new("❌ Back") {CallbackData = "switch " + typeof(MainMenu)}
        });
    }

    /// <summary>
    /// Generates buttons for navigation menu from list of menus
    /// </summary>
    protected void GenerateButtons(IEnumerable<Type> menuItems)
    {
        foreach (Type menuItem in menuItems)
        {
            var item = (MenuItem) Activator.CreateInstance(menuItem, new object[] {null});

            Buttons.Add(new List<InlineKeyboardButton>
            {
                new InlineKeyboardButton(item.ItemTitle) {CallbackData = $"switch {item.GetType()}"}
            });
        }
    }

    public InlineKeyboardMarkup GenerateMarkup()
    {
        GenerateButtons();
        return new InlineKeyboardMarkup(Buttons);
    }
}