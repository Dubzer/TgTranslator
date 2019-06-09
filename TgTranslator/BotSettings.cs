using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
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

            ShowMenu(settings[0]);
        }

        void ShowMenu(Setting mainMenu)
        {
            Program.botClient.SendTextMessageAsync(args.Message.Chat.Id, mainMenu.description, ParseMode.Markdown, true, false, 0, mainMenu.GenerateMarkup());
        }

        public static async Task SwitchItem(CallbackQueryEventArgs e)
        {
            Setting item = null;
            string arguments;

            if (e.CallbackQuery.Data.Contains(' '))
            {
                arguments = e.CallbackQuery.Data.Split(' ')[1];
                settings.Add(new ApplyMenu(arguments));
            }

            foreach (Setting obj in settings)
            {
                if (e.CallbackQuery.Data.Contains(obj.GetType().Name))
                {
                    item = obj;
                    break;
                }
            }

            await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, item.description, ParseMode.Markdown, true, item.GenerateMarkup());
        }
    }
}
