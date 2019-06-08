using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using TgTranslator.Settings;

namespace TgTranslator
{
    class BotSettings
    {
        List<Setting> settings;
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

        InlineKeyboardMarkup GenerateButtons()
        {
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            foreach (var setting in settings)
            {
                buttons.Add(new InlineKeyboardButton { Text = setting.itemTitle, CallbackData = setting.GetType().ToString() });
            }

            return new InlineKeyboardMarkup(buttons);
        }

        public static void SwitchItem(CallbackQueryEventArgs e, Setting item)
        {
            Program.botClient.EditMessageCaptionAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, item.message, item.GenerateButtons());
        }
    }
}
