using Telegram.Bot.Types;

namespace TgTranslator.Services;

public static class BotCommands
{
    public static readonly BotCommand[] PrivateChatCommands =
    {
        new()
        {
            Command = "contact",
            Description = "📩 Contact the developer"
        },
    };

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
}