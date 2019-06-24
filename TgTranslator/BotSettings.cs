using System;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TgTranslator.Settings;

namespace TgTranslator
{
    class BotSettings
    {
        private List<Setting> settings;

        public BotSettings()
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

        public async Task SwitchItem(CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.Data.Contains("MainMenu"))
            {
                MainMenu mainMenu = (MainMenu)FindSettingByName(e.CallbackQuery.Data.WithoutArguments(), settings);
                
                await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, mainMenu.description, ParseMode.Markdown, true, mainMenu.GenerateMarkup(settings));
                return;
            }
            
            if (e.CallbackQuery.Data.Contains("ApplyMenu"))
                settings.Add(new ApplyMenu(GetArguments(e)));
            

            Setting item = FindSettingByName(e.CallbackQuery.Data.WithoutArguments(), settings);
            await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, item.description, ParseMode.Markdown, true, item.GenerateMarkup());
        }

        private string GetArguments(CallbackQueryEventArgs e)
        {
            return e.CallbackQuery.Data.Split(' ')[1];
        }

        private Setting FindSettingByName(string name, IEnumerable<Setting> list)
        {
            return list.FirstOrDefault(x => x.GetType().ToString().Contains(name));
        }
    }
}