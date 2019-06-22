using System;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TgTranslator
{
    class UpdateHandler
    {
        private readonly BotSettings botSettings = new BotSettings();
        
        public async Task OnMessage(MessageEventArgs e)
        {
            switch (e.Message.Chat.Type)
            {
                case ChatType.Group:
                case ChatType.Supergroup:
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

                    if (Blacklists.Verify(language, Blacklists.LanguagesBlacklist))
                    {
                        string translation;

                        try
                        {
                            translation = await translator.TranslateText(e.Message.Text, language, ConfigurationProcessor.GetGroupLanguage(e.Message.Chat.Id));
                        }
                        catch (Exception exc)
                        {
                            LoggingService.Log($"Got an exception while tried to translate ({e.Message.Text}) \n\n {exc}");
                            return;
                        }

                        try
                        {
                            await Program.botClient.SendTextMessageAsync(e.Message.Chat.Id, translation, ParseMode.Default, true, true, e.Message.MessageId);
                            LoggingService.Log($"Sent translation to {e.Message.Chat.Title}");
                        }
                        catch (Exception exc)
                        {
                            LoggingService.Log($"Got an exception while tried to send ({translation}) to ({e.Message.Chat.Id})\n\n {exc}");
                        }
                    }

                    break;
                }

                case ChatType.Private:
                    switch (e.Message.Text)
                    {
                        case "/settings":
                            await botSettings.SendMenu(e.Message.Chat.Id);
                            break;
                        default:
                            
                            break;
                    }

                    break;
            }
        }

        public async Task OnCallbackQuery(CallbackQueryEventArgs e)
        {
            await botSettings.SwitchItem(e);
        }
    }
}
