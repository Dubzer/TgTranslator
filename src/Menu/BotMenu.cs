using System;
using System.Collections;
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
        private readonly ImmutableHashSet<Type> _availableMenus;
        
        public BotMenu(TelegramBotClient client)
        {
            _client = client;

            _availableMenus = new HashSet<Type>
            {
                typeof(MainMenu),
                typeof(ApplyMenu),
                typeof(BotMenu),
                typeof(Language),
                typeof(MainMenu)
            }.ToImmutableHashSet();
        }
        
        public async Task SendMainMenu(long chatId)
        {
            MainMenu menu = new MainMenu(null);
            await _client.SendTextMessageAsync(chatId, menu.description, 
                                                ParseMode.Markdown,  
                                                replyMarkup:menu.GenerateMarkup(_mainMenuItems));
        }

        public async Task SwitchMenu(Type menuType, string[] arguments, long chatId, int messageId)
        {
            MenuItem item = GetMenuItem(menuType, arguments);
            await _client.EditMessageTextAsync(chatId, messageId, item.description, 
                                                ParseMode.Markdown, 
                                                replyMarkup: item.GenerateMarkup());
        }

        private MenuItem GetMenuItem(Type menuType, IEnumerable arguments)
        {
            if(_availableMenus.Contains(menuType))
                return (MenuItem)Activator.CreateInstance(menuType, arguments);

            throw new UnsupportedMenuItem(menuType.ToString());
        }
                
    }
}