using System;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using BitFaster.Caching.Lru;
using Serilog;
using TgTranslator.Stats;

namespace TgTranslator.Services;

public class TranslatedMessagesCache
{
    public const int Capacity = 350;
    private readonly ILogger _logger;
    private readonly Metrics _metrics;

    public TranslatedMessagesCache(ILogger logger, Metrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    private readonly FastConcurrentLru<(int MessageId, long ChatId), (ulong TextHash, int TranslationId)> _cache = new(Capacity);

    public void AddTranslation(int messageId, long chatId, ReadOnlySpan<char> text, int translationId)
    {
        _logger.Information("Adding new translation entry. Current length: {Length}", _cache.Count);
        var textHash = GetHash(text);
        _cache.AddOrUpdate((messageId, chatId), (textHash, translationId));
        _metrics.TranslationCacheCounterInc();
    }

    /// <returns>
    /// null when does value does not require translation,
    /// otherwise returns the ID of translation message
    /// </returns>
    public int? RequiresTranslationUpdate(int messageId, long chatId, ReadOnlySpan<char> newText)
    {
        if (!_cache.TryGet((messageId, chatId), out var val))
            return null;

        var newTextHash = GetHash(newText);
        return val.TextHash == newTextHash
            ? null
            : val.TranslationId;
    }

    private static ulong GetHash(ReadOnlySpan<char> text)
    {
        var textBytes = MemoryMarshal.AsBytes(text);
        return XxHash3.HashToUInt64(textBytes);
    }
}