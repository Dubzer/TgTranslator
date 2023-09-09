﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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

    public Task SendStart(long chatId)
    {
        return _client.SendTextMessageAsync(
            chatId,
            "Hello!\nThis bot will translate messages in your group. It supports 134 languages and has different mods. You can check the demo settings page here\n\n" +
            "To get started, add to the group chat.\nHere's a shortcut button:",
            parseMode: ParseMode.Markdown,
            replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Select a group")
            {
                Url = $"https://t.me/{Program.Username}?startgroup=fromhelp"
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
        MenuItem item = GetMenuItem(menuType, arguments);
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