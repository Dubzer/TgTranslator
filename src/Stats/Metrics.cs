using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using TgTranslator.Services;

namespace TgTranslator.Stats;

public class Metrics : IDisposable
{
    internal const string MeterName = "TgTranslator";

    private readonly Meter _meter = new(MeterName);
    private readonly Counter<long> _groupCharacters;
    private readonly Counter<long> _groupMessages;
    private readonly Counter<long> _totalMessages;
    private readonly Counter<long> _translatorApiCalls;
    private readonly Counter<long> _translatorApiCharacters;
    private readonly Counter<long> _translationsCacheRotations;
    private readonly Counter<long> _translationEdits;
    private readonly Histogram<double> _translationResponseTime;

    private int _translationsCacheCounter;
    private readonly Lock _translationsCacheCounterLock = new();
    private long _totalGroups;
    private long _totalUsers;
    private long _totalPmUsers;

    public Metrics()
    {
        _totalMessages = _meter.CreateCounter<long>("total_messages", description: "Total messages");

        _groupMessages = _meter.CreateCounter<long>("group_messages", description: "Group messages");
        _groupCharacters = _meter.CreateCounter<long>("group_characters", description: "Group characters");

        _translatorApiCalls = _meter.CreateCounter<long>("translator_api_calls", description: "Translator API calls");
        _translatorApiCharacters = _meter.CreateCounter<long>("translator_api_characters", description: "Translator API characters");
        _translationsCacheRotations = _meter.CreateCounter<long>("translations_cache_rotations", description: "Translation cache rotations");
        _translationEdits = _meter.CreateCounter<long>("translation_edits", description: "Translation edits");

        _meter.CreateObservableGauge("total_groups", () => Interlocked.Read(ref _totalGroups), description: "Total groups count");
        _meter.CreateObservableGauge("total_users", () => Interlocked.Read(ref _totalUsers), description: "Total users count");
        _meter.CreateObservableGauge("total_pm_users", () => Interlocked.Read(ref _totalPmUsers), description: "Total PM users count");

        _translationResponseTime = _meter.CreateHistogram<double>("translation_response_time_ms", description: "Translation response time");
    }
        
    public void HandleMessage() => _totalMessages.Add(1);

    public void RecordTranslationEdit() => _translationEdits.Add(1);

    public void RecordTranslationResponseTime(long milliseconds) => _translationResponseTime.Record(milliseconds);

    public void SetTotalGroups(long count) => Interlocked.Exchange(ref _totalGroups, count);

    public void SetTotalUsers(long count) => Interlocked.Exchange(ref _totalUsers, count);

    public void SetTotalPmUsers(long count) => Interlocked.Exchange(ref _totalPmUsers, count);

    public void HandleGroupMessage(int charactersCount)
    {
        _groupMessages.Add(1);
        _groupCharacters.Add(charactersCount);
    }

    public void HandleTranslatorApiCall(int charactersCount, string sourceLanguage, string targetLanguage)
    {
        Span<string> pair = [sourceLanguage, targetLanguage];
        pair.Sort();

        var tags = new TagList
        {
            { "language_pair", $"{pair[0]}:{pair[1]}" }
        };
        _translatorApiCalls.Add(1, tags);
        _translatorApiCharacters.Add(charactersCount, tags);
    }

    public void TranslationCacheCounterInc()
    {
        lock (_translationsCacheCounterLock)
        {
            if (++_translationsCacheCounter >= TranslatedMessagesCache.Capacity)
            {
                _translationsCacheRotations.Add(1);
                _translationsCacheCounter = 0;
            }

        }
    }

    public void Dispose() => _meter.Dispose();
}
