using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TgTranslator.Settings;

namespace TgTranslator
{
    class SettingsMenu
    {
        private List<Setting> settings;

        public SettingsMenu()
        {
            settings = new List<Setting>
            {
                new MainMenu("Back"),
                new Language("Main language")
            };
        }

        public async Task SendMenu(long chatId)
        {
            MainMenu menu = (MainMenu)FindSettingByName("MainMenu", settings);
            await Program.botClient.SendTextMessageAsync(chatId, menu.description, ParseMode.Markdown, true, false, 0, menu.GenerateMarkup(settings));
        }

        public async Task SwitchItem(string itemName, long chatId, int messageId)
        {
            #region Exceptional cases 

            if (itemName.Contains("MainMenu"))
            {
                MainMenu mainMenu = (MainMenu)FindSettingByName(itemName.WithoutArguments(), settings);
                
                await Program.botClient.EditMessageTextAsync(chatId, messageId, mainMenu.description, ParseMode.Markdown, true, mainMenu.GenerateMarkup(settings));
                return;
            }

            if (itemName.Contains("ApplyMenu"))
            {
                Setting applyMenu = FindSettingByName(itemName.WithoutArguments(), settings);
                if (applyMenu != null)
                    settings.Remove(applyMenu);
                
                settings.Add(new ApplyMenu(GetArguments(itemName)));
            }

            #endregion
            

            Setting item = FindSettingByName(itemName.WithoutArguments(), settings);
            await Program.botClient.EditMessageTextAsync(chatId, messageId, item.description, ParseMode.Markdown, true, item.GenerateMarkup());
        }
        
        private string GetArguments(string itemName)
        {
            return itemName.Split(' ')[1];
        }

        private Setting FindSettingByName(string name, IEnumerable<Setting> list)
        {
            return list.FirstOrDefault(x => x.GetType().ToString().Contains(name));
        }
    }
}