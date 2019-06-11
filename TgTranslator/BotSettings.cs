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
        public static List<Setting> settings;
        private readonly MessageEventArgs args;

        public BotSettings(MessageEventArgs e)
        {
            args = e;
            settings = new List<Setting>()
            {
                new MainMenu("Back"),
                new Language("Main language")
            };

            SendMenu(settings[0]);
        }

        private void SendMenu(Setting mainMenu)
        {
            Program.botClient.SendTextMessageAsync(args.Message.Chat.Id, mainMenu.description, ParseMode.Markdown, true, false, 0, mainMenu.GenerateMarkup());
        }

        public static async Task SwitchItem(CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.Data.Contains("ApplyMenu"))
                settings.Add(new ApplyMenu(GetArguments(e)));

            Setting item = FindSettingByName(e.CallbackQuery.Data, settings);
            await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, item.description, ParseMode.Markdown, true, item.GenerateMarkup());
        }

        private static string GetArguments(CallbackQueryEventArgs e)
        {
            return e.CallbackQuery.Data.Split(' ')[1];
        }

        private static Setting FindSettingByName(string name, List<Setting> list)
        {
            return list.Where(x => x.GetType().ToString().Contains(name)).FirstOrDefault();
        }
    }
}