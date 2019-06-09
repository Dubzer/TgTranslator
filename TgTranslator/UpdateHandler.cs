using System;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TgTranslator
{
    class UpdateHandler
    {
        public static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == ChatType.Group || e.Message.Chat.Type == ChatType.Supergroup)
            {
                LoggingService.Log($"Got: [ {e.Message.Text} ] by {e.Message.From.Username} from {e.Message.Chat.Title}");
                Translator translator = new Translator(Program.Configuration["tokens:yandex"]);
                string language;

                try
                {
                    language = await translator.DetectLanguage(e.Message.Text);
                }
                catch (Exception exc)
                {
                    LoggingService.Log($"Got an exception while tried to detect lang ({e.Message.Text}) \n\n {exc}");
                    return;
                }

                if (Blacklists.Verify(language, Blacklists.languagesBlacklist))
                {
                    string translation;

                    try
                    {
                        translation = await translator.TranslateText(e.Message.Text, language, "en");
                    }
                    catch (Exception exc)
                    {
                        LoggingService.Log($"Got an exception while tried to translate ({e.Message.Text}) \n\n {exc}");
                        return;
                    }

                    try
                    {
                        await Program.botClient.SendTextMessageAsync(e.Message.Chat.Id, translation, Telegram.Bot.Types.Enums.ParseMode.Default, true, true, e.Message.MessageId);
                        LoggingService.Log($"Sent translation to {e.Message.Chat.Title}");
                    }
                    catch (Exception exc)
                    {
                        LoggingService.Log($"Got an exception while tried to send ({translation}) to ({e.Message.Chat.Id})\n\n {exc}");
                        return;
                    }
                }
            }
            else if (e.Message.Chat.Type == ChatType.Private)
            {
                new BotSettings(e);
            }
        }

        public static async void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            await BotSettings.SwitchItem(e);
        }
    }
}
