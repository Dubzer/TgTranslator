using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgTranslator.Menu;

namespace TgTranslator.Services;

public class CommandsManager
{
    private readonly TelegramBotClient _botClient;

    public CommandsManager(TelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task SetDefaultCommands()
    {
        // pm
        await _botClient.SetMyCommandsAsync(
        [
            BotCommands.SettingsCommand,
            BotCommands.ContactCommand
        ], BotCommandScope.AllPrivateChats());

        // group chat administrators
        await _botClient.SetMyCommandsAsync([BotCommands.SettingsCommand], BotCommandScope.AllChatAdministrators());
    }

    public async Task ChangeGroupMode(ChatId chatId, TranslationMode translationMode)
    {
        switch (translationMode)
        {
            case TranslationMode.Manual:
                await _botClient.SetMyCommandsAsync([
                    BotCommands.SettingsCommand,
                    BotCommands.TranslateCommand
                ], BotCommandScope.ChatAdministrators(chatId));

                await _botClient.SetMyCommandsAsync([
                    BotCommands.TranslateCommand
                ], BotCommandScope.Chat(chatId));
                break;
            case TranslationMode.Auto:
            case TranslationMode.Forwards:
            case TranslationMode.LinkedChannel:
            default:
                // the default commands will be shown after deleting the scoped ones
                await _botClient.DeleteMyCommandsAsync(BotCommandScope.ChatAdministrators(chatId));
                await _botClient.DeleteMyCommandsAsync(BotCommandScope.Chat(chatId));
                break;
        }
    }
}