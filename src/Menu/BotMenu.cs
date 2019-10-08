using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TgTranslator.Exceptions;

namespace TgTranslator.Menu
{
    public class BotMenu
    {
        private readonly TelegramBotClient _client;
        private List<Type> _mainMenuItems;
        private ImmutableHashSet<Type> _availableMenus;
        
        public BotMenu(TelegramBotClient client)
        {
            _client = client;
            
            // Menus that shows in Main Menu
            _mainMenuItems = new List<Type>
            {
                typeof(Language)
            };
            
            _availableMenus = new HashSet<Type>
            {
                typeof(ApplyMenu),
                typeof(BotMenu),
                typeof(Language),
                typeof(MainMenu)
            }.ToImmutableHashSet();
        }
        
        public async Task SendSettingsMenu(long chatId)
        {
            MainMenu menu = new MainMenu();
            await _client.SendTextMessageAsync(chatId, menu.description, 
                                                ParseMode.Markdown,  
                                                replyMarkup:menu.GenerateMarkup(_mainMenuItems));
        }

        public async Task SwitchMenu(Type menuType, string[] arguments, long chatId, int messageId)
        {
            MenuItem item = GetMenuItem(menuType, arguments);
            await _client.EditMessageTextAsync(chatId, messageId, item.description, 
                                                ParseMode.Markdown, 
                                                replyMarkup: item.GenerateMarkup(
                                                    item.GetType().ToString().Contains("MainMenu") 
                                                        ? _mainMenuItems 
                                                        : null));
        }

        private MenuItem GetMenuItem(Type menuType, string[] arguments)
        {
            if(_availableMenus.Contains(menuType))
                return (MenuItem)Activator.CreateInstance(menuType, new object[] {arguments});

            throw new UnsupportedMenuItem(menuType.ToString());
        }
                
    }
}