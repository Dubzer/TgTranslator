using System.Globalization;
using Telegram.Bot.Types;
using TgTranslator.Resources;

namespace TgTranslator.Services;

public class BotCommands(CultureInfo culture)
{
    private readonly Localization _localization = new(culture);

    public BotCommand SettingsCommand => new()
    {
        Command = "settings",
        Description = _localization.SettingsDescription
    };

    public BotCommand TranslateCommand => new()
    {
        Command = "tl",
        Description = _localization.TranslateDescription
    };

    public BotCommand ContactCommand => new()
    {
        Command = "contact",
        Description = _localization.ContactDescription
    };

    public BotCommand DonateCommand => new()
    {
        Command = "donate",
        Description = _localization.DonateDescription
    };
}