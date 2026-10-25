using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Services.EventHandlers.Messages;
using TgTranslator.Stats;
using TgTranslator.Utils.Extensions;

namespace TgTranslator.Services.EventHandlers;

public class EditedMessageHandler
{
    private readonly TelegramBotClient _client;
    private readonly TranslatedMessagesCache _translatedMessagesCache;
    private readonly TranslateHandler _translateHandler;
    private readonly SettingsService _settings;
    private readonly Metrics _metrics;

    public EditedMessageHandler(TranslatedMessagesCache translatedMessagesCache, TranslateHandler translateHandler, SettingsService settings, TelegramBotClient client, Metrics metrics)
    {
        _translatedMessagesCache = translatedMessagesCache;
        _translateHandler = translateHandler;
        _settings = settings;
        _client = client;
        _metrics = metrics;
    }

    public async Task Handle(Message message)
    {
        var text = message.TextOrCaption;
        if (text == null)
            return;
        
        var cached = _translatedMessagesCache
            .RequiresTranslationUpdate(message.MessageId, message.Chat.Id, text);

        if (cached == null)
            return;

        var translationId = cached.Value;
        var groupSettings = await _settings.GetSettings(message.Chat.Id);

        var translation = await _translateHandler.TranslateAndFix(message, text, groupSettings);
        if (string.IsNullOrWhiteSpace(translation) || translation.Length > 4096)
            return;

        try
        {
            await _client.EditMessageText(message.Chat.Id, translationId, translation, ParseMode.Html);
        }
        catch (ApiRequestException exception) when (exception.IsMessageToEditNotFound())
        {
            _translatedMessagesCache.RemoveTranslation(message.MessageId, message.Chat.Id);
            return;
        }

        _metrics.RecordTranslationEdit();
    }
}
