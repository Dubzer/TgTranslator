using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgTranslator.Exceptions;
using TgTranslator.Interfaces;
using TgTranslator.Menu;

namespace TgTranslator.Services.Handlers;

public class CallbackQueryHandler : ICallbackQueryHandler
{
    private readonly BotMenu _botMenu;
    private readonly TelegramBotClient _client;

    // Key is command, value means is command require arguments
    private readonly ImmutableDictionary<string, bool> _supportedCommands;

    public CallbackQueryHandler(BotMenu botMenu, TelegramBotClient client)
    {
        _botMenu = botMenu;
        _client = client;

        _supportedCommands = new Dictionary<string, bool>
        {
            {"switch", true}
        }.ToImmutableDictionary();
    }

    #region ICallbackQueryHandler Members

    public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
    {
        string data = callbackQuery.Data;

        if (string.IsNullOrWhiteSpace(data))
            return;

        // [0] - command [1] - arguments
        string[] parts = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string command = parts[0];
        string[] arguments = parts.Length > 1 //  If there are any arguments
            ? parts[1].Split('#')
            : null;

        if (!_supportedCommands.TryGetValue(command, out bool value) || value != (arguments != null))
            throw new UnsupportedCommand(callbackQuery.Data);

        switch (command)
        {
            case "switch" when arguments != null:
                await _botMenu.SwitchMenu(Type.GetType(arguments[0]),
                    arguments.Skip(1).ToArray(),
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId);
                break;
        }

        await _client.AnswerCallbackQueryAsync(callbackQuery.Id);
    }

    #endregion
}