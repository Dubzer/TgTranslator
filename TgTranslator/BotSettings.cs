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
        static List<Setting> settings;
        MessageEventArgs args;

        public BotSettings(MessageEventArgs e)
        {
            args = e;
            settings = new List<Setting>()
            {
                new Language("Language")
            };

            ShowMenu();
        }

        void ShowMenu()
        {
            Program.botClient.SendTextMessageAsync(args.Message.Chat, "That's some text", Telegram.Bot.Types.Enums.ParseMode.Markdown, true, false, 0, GenerateButtons());
            Console.WriteLine("Sent message with settings menu to " + args.Message.From.Username);
        }

        static InlineKeyboardMarkup GenerateButtons()
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            foreach (var setting in settings)
            {
                buttons.Add(new InlineKeyboardButton { Text = setting.itemTitle, CallbackData = setting.GetType().ToString() });
            }

            return new InlineKeyboardMarkup(buttons);
        }

        public static async Task SwitchItem(CallbackQueryEventArgs e)
        {
            Setting item = null;

            foreach (Setting obj in settings)
            {
                if (obj.GetType().ToString() == e.CallbackQuery.Data)
                {
                    item = obj;
                    break;
                }
            }

            await Program.botClient.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, item.description, ParseMode.Markdown, true, item.GenerateMarkup());
        }
    }
}
