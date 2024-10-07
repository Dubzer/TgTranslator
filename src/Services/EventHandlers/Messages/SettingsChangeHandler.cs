using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgTranslator.Exceptions;
using TgTranslator.Menu;
using TgTranslator.Utils.Extensions;

namespace TgTranslator.Services.EventHandlers.Messages;

public class SettingsChangeHandler
{
    private readonly TelegramBotClient _client;
    private readonly SettingsService _settingsService;

    public SettingsChangeHandler(TelegramBotClient client, SettingsService settingsService)
    {
        _client = client;
        _settingsService = settingsService;
    }

    public async Task Handle(Message message)
    {
        if (!await message.From.IsAdministrator(message.Chat.Id, _client))
            throw new UnauthorizedSettingChangingException();

        string tinyString = message.Text!.Replace($"@{Static.Username} set:", "");

        (string param, string value) = (tinyString.Split('=')[0], tinyString.Split('=')[1]);

        if (!Enum.TryParse(param, true, out Setting setting) || !Enum.IsDefined(typeof(Setting), setting))
            throw new InvalidSettingException();

        if (!_settingsService.ValidateSettings(Enum.Parse<Setting>(param, true), value))
            throw new InvalidSettingValueException();

        switch (setting)
        {
            case Setting.Language:
                await _settingsService.SetLanguage(message.Chat.Id, value);
                break;
            case Setting.Mode:
                var mode = Enum.Parse<TranslationMode>(value, true);
                await _settingsService.SetMode(message.Chat.Id, mode);
                break;
        }

        await _client.SendTextMessageAsync(
            message.Chat.Id,
            "Done!",
            replyParameters: new ReplyParameters
            {
                MessageId = message.MessageId,
                AllowSendingWithoutReply = false,
            });
    }
}