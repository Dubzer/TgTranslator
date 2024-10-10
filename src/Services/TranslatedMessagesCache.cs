using System;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using BitFaster.Caching.Lru;
using Serilog;

namespace TgTranslator.Services;

public class TranslatedMessagesCache
{
    private readonly ILogger _logger;

    public TranslatedMessagesCache(ILogger logger)
    {
        _logger = logger;
    }

    private readonly FastConcurrentLru<(int MessageId, long ChatId), (ulong TextHash, int TranslationId)> _cache = new(350);

    public void AddTranslation(int messageId, long chatId, ReadOnlySpan<char> text, int translationId)
    {
        _logger.Information("Adding new translation entry. Current length: {Length}", _cache.Count);
        var textHash = GetHash(text);
        _cache.AddOrUpdate((messageId, chatId), (textHash, translationId));
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