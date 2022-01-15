using Telegram.Bot.Types;

namespace TgTranslator.Services;

public static class BotCommands
{
    public static readonly BotCommand AdminCommand = new()
    {
        Command = "settings",
        Description = "Change language and mode."
    };

    public static readonly BotCommand TranslateCommand = new()
    {
        Command = "tl",
        Description = "Translate replied message."
    };
}