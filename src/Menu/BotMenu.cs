using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TgTranslator.Data.Options;
using TgTranslator.Exceptions;

namespace TgTranslator.Menu;

public class BotMenu
{
    private const string HelpCaption =
        "To start using a bot, simply add it to the group as on the video. Then you can change some settings here.";

    private readonly ImmutableHashSet<Type> _availableMenus;
    private readonly TelegramBotClient _client;
    private readonly string _helpVideoUrl;

    public BotMenu(TelegramBotClient client, IOptions<HelpmenuOptions> helpmenuOptions)
    {
        _client = client;
        _helpVideoUrl = helpmenuOptions.Value.VideoUrl;

        _availableMenus = new HashSet<Type>
        {
            typeof(MainMenu),
            typeof(ApplyMenu),
            typeof(BotMenu),
            typeof(LanguageMenu),
            typeof(MainMenu),
            typeof(ModeMenu)
        }.ToImmutableHashSet();
    }

    public async Task SendMainMenu(long chatId)
    {
        var menu = new MainMenu(null);
        await _client.SendTextMessageAsync(chatId, menu.Description,
            ParseMode.Markdown,
            replyMarkup: menu.GenerateMarkup());
    }

    public async Task SwitchMenu(Type menuType, string[] arguments, long chatId, int messageId)
    {
        MenuItem item = GetMenuItem(menuType, arguments);
        await _client.EditMessageTextAsync(chatId, messageId, item.Description,
            ParseMode.Markdown,
            replyMarkup: item.GenerateMarkup());
    }

    public async Task SendHelpMenu(long chatId) => await _client.SendVideoAsync(chatId, _helpVideoUrl, caption: HelpCaption);

    private MenuItem GetMenuItem(Type menuType, IEnumerable arguments)
    {
        if (_availableMenus.Contains(menuType))
            return (MenuItem) Activator.CreateInstance(menuType, arguments);

        throw new UnsupportedMenuItem(menuType.ToString());
    }
}