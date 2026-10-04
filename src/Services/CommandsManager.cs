using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgTranslator.Menu;
using TgTranslator.Resources;

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
        foreach (var languageCode in Localization.SupportedLanguages)
        {
            var commands = new BotCommands(CultureInfo.GetCultureInfo(languageCode));

            // pm
            await _botClient.SetMyCommands(
            [
                commands.SettingsCommand,
                commands.ContactCommand,
                commands.DonateCommand
            ], BotCommandScope.AllPrivateChats(), languageCode: languageCode);

            // group chat administrators
            await _botClient.SetMyCommands([commands.SettingsCommand],
                BotCommandScope.AllChatAdministrators(), languageCode: languageCode);
        }
    }

    public async Task SetBotDescriptions(CancellationToken cancellationToken = default)
    {
        foreach (var languageCode in Localization.SupportedLanguages)
        {
            var localization = new Localization(CultureInfo.GetCultureInfo(languageCode));
            await _botClient.SetMyDescription(localization.BotDescription,
                languageCode: languageCode, cancellationToken: cancellationToken);
            await _botClient.SetMyShortDescription(localization.BotDescription,
                languageCode: languageCode, cancellationToken: cancellationToken);
        }
    }

    public async Task ChangeGroupMode(ChatId chatId, TranslationMode translationMode)
    {
        foreach (var languageCode in Localization.SupportedLanguages)
        {
            if (translationMode == TranslationMode.Manual)
            {
                var commands = new BotCommands(CultureInfo.GetCultureInfo(languageCode));

                await _botClient.SetMyCommands([
                    commands.SettingsCommand,
                    commands.TranslateCommand
                ], BotCommandScope.ChatAdministrators(chatId), languageCode: languageCode);

                await _botClient.SetMyCommands([
                    commands.TranslateCommand
                ], BotCommandScope.Chat(chatId), languageCode: languageCode);
            }
            else
            {
                // the default commands will be shown after deleting the scoped ones
                await _botClient.DeleteMyCommands(BotCommandScope.ChatAdministrators(chatId),
                    languageCode: languageCode);
                await _botClient.DeleteMyCommands(BotCommandScope.Chat(chatId), languageCode: languageCode);
            }
        }
    }
}
