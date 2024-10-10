#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Data.DTO;
using TgTranslator.Interfaces;
using TgTranslator.Menu;
using TgTranslator.Stats;
using TgTranslator.Utils;
using TgTranslator.Utils.Extensions;

namespace TgTranslator.Services.EventHandlers.Messages;

public partial class TranslateHandler
{
    [GeneratedRegex(@"\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff]", RegexOptions.Compiled)]
    private static partial Regex Emoji();
    [GeneratedRegex(@"\p{P}|\d|\s", RegexOptions.Compiled)]
    private static partial Regex NonLetters();

    private readonly ILogger _logger;
    private readonly TelegramBotClient _client;
    private readonly ITranslator _translator;
    private readonly Metrics _metrics;
    private readonly TranslatedMessagesCache _translatedMessagesCache;

    public TranslateHandler(ILogger logger, TelegramBotClient client, ITranslator translator, Metrics metrics, TranslatedMessagesCache translatedMessagesCache)
    {
        _logger = logger;
        _client = client;
        _translator = translator;
        _metrics = metrics;
        _translatedMessagesCache = translatedMessagesCache;
    }

    public async Task Handle(Message message, string originalText, Settings groupConfig)
    {
        _logger.Information("Handling translation for {ChatId} | {From}...", message.Chat.Id, message.From);
        _metrics.HandleTranslatorApiCall(message.Chat.Id, string.IsNullOrEmpty(originalText) ? 0 : originalText.Length);

        // a setting that prevents automatically translating messages with links
        if (!groupConfig.TranslateWithLinks
            && groupConfig.TranslationMode is TranslationMode.Auto or TranslationMode.Forwards
            && message.Entities?.Any(x => x.Type is MessageEntityType.TextLink or MessageEntityType.Url) == true)
            return;

        if (!originalText.AnyLetters())
            return;

        if (groupConfig.Delay != 0 && groupConfig.TranslationMode == TranslationMode.Auto)
            await Task.Delay(TimeSpan.FromSeconds(groupConfig.Delay));

        var translatedText = await TranslateAndFix(message, originalText, groupConfig);
        if (translatedText == null)
            return;

        // In case of multiple translation messages, will contain the last one sent
        Message translationMessage;
        if (translatedText.Length <= 4096)
        {
            translationMessage = await _client.SendTextMessageAsync(message.Chat.Id, translatedText,
                disableNotification: true, linkPreviewOptions: TelegramUtils.DisabledLinkPreview,
                replyParameters: TelegramUtils.SafeReplyTo(message.MessageId));

            _translatedMessagesCache.AddTranslation(message.MessageId, message.Chat.Id, originalText, translationMessage.MessageId);
        }
        else
        {
            translationMessage = await SendLongMessage(message.Chat.Id, translatedText, message.MessageId);
        }


        // The time between the original message and the translation message
        var translationMs = ((DateTimeOffset)translationMessage.Date).ToUnixTimeMilliseconds() -
                            ((DateTimeOffset)message.Date).ToUnixTimeMilliseconds();

        _metrics.TranslationResponseTime.Observe(translationMs);
        if (translationMs > 10000)
            _logger.Warning("Abnormal translation time for {ChatId} | {From} | {Time}ms",
                message.Chat.Id,
                message.From,
                translationMs);

        _logger.Information("Sent translation to {ChatId} | {From}", message.Chat.Id, message.From);
    }

    public async Task<string?> TranslateAndFix(Message message, string originalText, Settings groupConfig)
    {
        var translation = await _translator.TranslateTextAsync(originalText, groupConfig.Languages[0]);
        var translatedText = translation.Text;

        if (translation.DetectedLanguage == groupConfig.Languages[0] || !TranslationHappened(originalText, translatedText))
            return null;

        translatedText = TranslationUtils.FixEntities(originalText, translatedText, message.Entities);
        return translatedText;
    }

    // This method compares the result of the translation to original text,
    // while preventing some of the hallucinations of the translation service
    private static bool TranslationHappened(string originalText, string translatedText)
    {
        var emojiRegex = Emoji();
        var nonLettersRegex = NonLetters();

        var normalizedText = nonLettersRegex.Replace(originalText, "");
        normalizedText = emojiRegex.Replace(normalizedText, "");

        var normalizedTranslation = nonLettersRegex.Replace(translatedText, "");
        normalizedTranslation = emojiRegex.Replace(normalizedTranslation, "");

        return !string.IsNullOrEmpty(normalizedTranslation)
               && !string.Equals(normalizedText, normalizedTranslation, StringComparison.InvariantCultureIgnoreCase);
    }

    private async Task<Message> SendLongMessage(long chatId, string message, int replyId)
    {
        var (firstPart, secondPart) = MessageSplitter.Split(message);

        var firstPartResult = await _client.SendTextMessageAsync(chatId, firstPart,
            disableNotification: true, linkPreviewOptions: TelegramUtils.DisabledLinkPreview,
            replyParameters: TelegramUtils.SafeReplyTo(replyId));

        return await _client.SendTextMessageAsync(chatId, secondPart,
            disableNotification: true, linkPreviewOptions: TelegramUtils.DisabledLinkPreview,
            replyParameters: TelegramUtils.SafeReplyTo(firstPartResult.MessageId));
    }
}