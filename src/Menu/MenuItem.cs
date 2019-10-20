using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Menu
{ 
    public abstract class MenuItem
    {
        protected List<IEnumerable<InlineKeyboardButton>> buttons = new List<IEnumerable<InlineKeyboardButton>>();
        public string description;
        protected string itemTitle;
        protected string command;
        /// <summary>
        /// Generates default buttons
        /// </summary>
        protected virtual void GenerateButtons()
        {
            buttons.Add(new List<InlineKeyboardButton>
                        {
                            new InlineKeyboardButton { Text = "Back to the menu", CallbackData = "switch " + typeof(MainMenu)}
                        });
        }

        /// <summary>
        /// Generates buttons for navigation menu from list of menus
        /// </summary>
        protected void GenerateButtons(IEnumerable<Type> menuItems)
        {
            foreach (var menuItem in menuItems)
            {
                var item = (MenuItem)Activator.CreateInstance(menuItem, new object[] {null});
                
                buttons.Add(new List<InlineKeyboardButton>
                {
                    new InlineKeyboardButton { Text = item.itemTitle, CallbackData = $"switch {item.GetType()}" } 
                    
                });
            }
        }

        public InlineKeyboardMarkup GenerateMarkup()
        {
            GenerateButtons();

            return new InlineKeyboardMarkup(buttons);
        }
    }
}
