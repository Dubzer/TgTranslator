using System;
using System.Threading.Tasks;
using Extensions;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TgTranslator
{
    class UpdateHandler
    {
        private readonly SettingsMenu settingsMenu = new SettingsMenu();
        private readonly SettingsProcessor settingsProcessor = new SettingsProcessor();
        
        public async Task OnMessage(MessageEventArgs e)
        {
            switch (e.Message.Chat.Type)
            {
                case ChatType.Group:
                case ChatType.Supergroup:
                {   
                    LoggingService.Log($"Got: [ {e.Message.Text} ] by {e.Message.From.Username} from {e.Message.Chat.Title}");
                    
                    if (e.Message.Text.Contains($"{Program.BotClient.GetMeAsync().Result.Username} set:lang"))
                    {
                        if (!await e.Message.From.IsAdministrator(e.Message.Chat.Id))
                        {
                            await Program.BotClient.SendTextMessageAsync(e.Message.Chat.Id, "Hey, only admins can change settings of this bot!", ParseMode.Default, false, true, e.Message.MessageId);
                        }

                        string tinyString = e.Message.Text.Replace($"{Program.BotClient.GetMeAsync().Result.Username} set:", "");
                        
                        string param = tinyString.Split('=')[0];
                        string value = tinyString.Split('=')[1];
                        if (!settingsProcessor.ValidateLanguage(value, Program.languages))
                        {
                            await Program.BotClient.SendTextMessageAsync(e.Message.Chat.Id, "It seems that this language is not supported", ParseMode.Default, false, true, e.Message.MessageId);
                            return;
                        }
                        settingsProcessor.ChangeSetting(e.Message.Chat.Id, param, value);
                        await Program.BotClient.SendTextMessageAsync(e.Message.Chat.Id, "Done!", ParseMode.Default, false, true, e.Message.MessageId);

                        break;
                    }
                    
                    
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

                    if (language != settingsProcessor.GetGroupLanguage(e.Message.Chat.Id))
                    {
                        string translation;

                        try
                        {
                            translation = await translator.TranslateText(e.Message.Text, language, settingsProcessor.GetGroupLanguage(e.Message.Chat.Id));
                        }
                        catch (Exception exc)
                        {
                            LoggingService.Log($"Got an exception while tried to translate ({e.Message.Text}) \n\n {exc}");
                            return;
                        }

                        try
                        {
                            await Program.BotClient.SendTextMessageAsync(e.Message.Chat.Id, translation, ParseMode.Default, true, true, e.Message.MessageId);
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
                        case "/start":
                            await Program.BotClient.SendTextMessageAsync(e.Message.Chat.Id, "You should to add this bot in a group. Then, you can configure it in this chat.");
                            break;
                        case "/settings":
                            await settingsMenu.SendMenu(e.Message.Chat.Id);
                            break;
                        default:
                            await Program.BotClient.SendTextMessageAsync(e.Message.Chat.Id, "Unknown command. Type /start to get help");
                            break;
                    }

                    break;
            }
        }

        public async Task OnCallbackQuery(CallbackQueryEventArgs e)
        {
            string command = e.CallbackQuery.Data.Split(' ')[0];

            try
            {
                switch (command)
                {
                    case "switch:":
                        await settingsMenu.SwitchItem(e.CallbackQuery.Data.Replace($@"{command} ", ""), e.CallbackQuery.Message.Chat.Id,e.CallbackQuery.Message.MessageId);
                        break;
                }  
            }
            catch (Exception exception)
            {
                LoggingService.Log($"Got an exception while tried to process query by {e.CallbackQuery.From.Username} ({e.CallbackQuery.Data} \n\n {exception.Message}");
                throw;
            }
  
            
        }
    }
}
