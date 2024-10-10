using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
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
        var cached = _translatedMessagesCache
            .RequiresTranslationUpdate(message.MessageId, message.Chat.Id, message.TextOrCaption());

        if (cached == null)
            return;

        var translationId = cached.Value;
        var groupSettings = await _settings.GetSettings(message.Chat.Id);

        var translation = await _translateHandler.TranslateAndFix(message, message.TextOrCaption(), groupSettings);
        if (translation == null || translation.Length > 4096)
            return;

        await _client.EditMessageTextAsync(message.Chat.Id, translationId, translation);
        _metrics.TranslationEdits.Inc();
    }
}