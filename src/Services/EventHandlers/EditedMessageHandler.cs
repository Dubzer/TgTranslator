using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Services.EventHandlers.Messages;
using TgTranslator.Stats;
using TgTranslator.Utils;
using TgTranslator.Utils.Extensions;

namespace TgTranslator.Services.EventHandlers;

public class EditedMessageHandler
{
    private readonly TelegramBotClient _client;
    private readonly TranslatedMessagesCache _translatedMessagesCache;
    private readonly TranslateHandler _translateHandler;
    private readonly SettingsService _settings;
    private readonly Metrics _metrics;
    private readonly ILogger<EditedMessageHandler> _logger;

    public EditedMessageHandler(TranslatedMessagesCache translatedMessagesCache, TranslateHandler translateHandler, SettingsService settings, TelegramBotClient client, Metrics metrics, ILogger<EditedMessageHandler> logger)
    {
        _translatedMessagesCache = translatedMessagesCache;
        _translateHandler = translateHandler;
        _settings = settings;
        _client = client;
        _metrics = metrics;
        _logger = logger;
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
        if (string.IsNullOrWhiteSpace(translation))
            return;

        try
        {
            if (TranslationUtils.TryCreateRichMessage(translation, out var richMessage))
            {
                await _client.EditMessageText(message.Chat.Id, translationId, text: null, richMessage: richMessage);
            }
            else
            {
                await _client.EditMessageText(message.Chat.Id, translationId, translation, ParseMode.Html);
            }

            _metrics.RecordTranslationEdit();
        }
        catch (ApiRequestException exception) when (exception.ErrorCode == 400 &&
            exception.Message.StartsWith("Bad Request: message is not modified", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Translation {TranslationId} is already up to date: {TelegramErrorCode} {TelegramDescription}",
                translationId, exception.ErrorCode, exception.Message);
        }
        catch (ApiRequestException exception) when (exception.IsMessageToEditNotFound())
        {
            _translatedMessagesCache.RemoveTranslation(message.MessageId, message.Chat.Id);
            _logger.LogInformation("Removed cached translation {TranslationId} because Telegram could not find the message to edit: {TelegramErrorCode} {TelegramDescription}",
                translationId, exception.ErrorCode, exception.Message);
            return;
        }

        _translatedMessagesCache.UpdateTranslation(message.MessageId, message.Chat.Id, text, translationId);
    }
}
