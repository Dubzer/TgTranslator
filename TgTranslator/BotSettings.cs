using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TgTranslator.Settings;

namespace TgTranslator
{
    class BotSettings
    {
        public static List<Setting> settings;
        MessageEventArgs args;

        public BotSettings(MessageEventArgs e)
        {
            args = e;
            settings = new List<Setting>()
            {
                new MainMenu("Back"),
                new Language("Language")
            };

            SendMenu(settings[0]);
        }

        
        void SendMenu(Setting mainMenu)
        {
            Program.botClient.SendTextMessageAsync(args.Message.Chat.Id, mainMenu.description, ParseMode.Markdown, true, false, 0, mainMenu.GenerateMarkup());
        }

        public static async Task SwitchItem(CallbackQueryEventArgs e)
        {
            Setting item = null;

            foreach (Setting obj in settings)
            {
                if (obj.GetType().ToString().Contains(e.CallbackQuery.Data))
                {
                    item = obj;
                    break;
                }
            }

            await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, item.description, ParseMode.Markdown, true, item.GenerateMarkup());
        }
    }
}
