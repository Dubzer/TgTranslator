using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgTranslator.Services;

public static class BotCommands
{
    public static readonly BotCommand SettingsCommand = new()
    {
        Command = "settings",
        Description = "⚙️ Change language and mode"
    };

    public static readonly BotCommand TranslateCommand = new()
    {
        Command = "tl",
        Description = "🌐 Translate replied message."
    };

    public static async Task SetDefaultSettings(ITelegramBotClient client)
    {
        await client.SetMyCommandsAsync(
        [
            SettingsCommand,
            new()
            {
                Command = "contact",
                Description = "📩 Contact the developer"
            }
        ], BotCommandScope.AllPrivateChats());

        await client.SetMyCommandsAsync([SettingsCommand], BotCommandScope.AllChatAdministrators());
    }
}