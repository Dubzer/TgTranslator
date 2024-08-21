using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TgTranslator.Exceptions;
using TgTranslator.Utils;

namespace TgTranslator.Menu;

public class BotMenu
{
    private readonly ImmutableHashSet<Type> _availableMenus;
    private readonly TelegramBotClient _client;

    public BotMenu(TelegramBotClient client)
    {
        _client = client;

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

    public Task SendStart(long chatId)
    {
        return _client.SendTextMessageAsync(
            chatId,
            "🦈 Hello!\nThis bot will translate messages in your group. It supports 134 languages and has various modes. Check out the demo settings page [here](https://t.me/TgTranslatorBot/settings?startapp=mock).\n\n" +
            "To get started, add it to the group chat.",
            parseMode: ParseMode.Markdown,
            linkPreviewOptions: TelegramUtils.DisabledLinkPreview,
            replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Select a group")
            {
                Url = $"https://t.me/{Static.Username}?startgroup=frommenu"
            }));
    }

    public async Task SendSettings(long chatId)
    {
        var menu = new MainMenu(null);
        await _client.SendTextMessageAsync(chatId, menu.Description,
            parseMode: ParseMode.Markdown,
            replyMarkup: menu.GenerateMarkup());
    }

    public async Task SwitchMenu(Type menuType, string[] arguments, long chatId, int messageId)
    {
        var item = GetMenuItem(menuType, arguments);
        await _client.EditMessageTextAsync(chatId, messageId, item.Description,
            ParseMode.Markdown,
            replyMarkup: item.GenerateMarkup());
    }

    private MenuItem GetMenuItem(Type menuType, IEnumerable arguments)
    {
        if (_availableMenus.Contains(menuType))
            return (MenuItem) Activator.CreateInstance(menuType, arguments);

        throw new UnsupportedMenuItem(menuType.ToString());
    }
}