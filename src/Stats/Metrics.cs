using System.Linq;
using System.Threading;
using Prometheus;
using TgTranslator.Services;

namespace TgTranslator.Stats;

public class Metrics
{
    private readonly Counter _groupCharacters;
    private readonly Counter _groupMessages;
    private readonly Counter _totalMessages;
    private readonly Counter _translatorApiCalls;
    private readonly Counter _translatorApiCharacters;
    private readonly Counter _translationsCacheRotations;
    public readonly Counter TranslationEdits;
    public readonly Gauge TotalGroups;
    public readonly Gauge TotalUsers;
    public readonly Gauge TotalPmUsers;
    public readonly Histogram TranslationResponseTime;

    private int _translationsCacheCounter;
    private readonly object _translationsCacheCounterLock = new();

    public Metrics()
    {
        _totalMessages = Prometheus.Metrics.CreateCounter("total_messages", "Total messages");

        _groupMessages = Prometheus.Metrics.CreateCounter("group_messages", "Group messages", "group_id");
        _groupCharacters = Prometheus.Metrics.CreateCounter("group_characters", "Group Characters", "group_id");

        _translatorApiCalls = Prometheus.Metrics.CreateCounter("translator_api_calls", "Translator API calls", "group_id");
        _translatorApiCharacters = Prometheus.Metrics.CreateCounter("translator_api_characters", "Translator API characters", "group_id");
        _translationsCacheRotations = Prometheus.Metrics.CreateCounter("translations_cache_rotations", "Translation cache rotations");
        TranslationEdits = Prometheus.Metrics.CreateCounter("translation_edits", "Translation edits");

        TotalGroups = Prometheus.Metrics.CreateGauge("total_groups", "Total groups count");
        TotalUsers = Prometheus.Metrics.CreateGauge("total_users", "Total users count");
        TotalPmUsers = Prometheus.Metrics.CreateGauge("total_pm_users", "Total PM users count");

        TranslationResponseTime = Prometheus.Metrics.CreateHistogram("translation_response_time_ms", "Translation response time", new HistogramConfiguration
        {
            Buckets = [..Enumerable.Range(1, 7).Select(x => x * 250), 3000, 5000, 8000]
        });
    }
        
    public void HandleMessage() => TotalMessagesInc();

    public void HandleGroupMessage(long groupId, int charactersCount)
    {
        GroupMessagesInc(groupId);
        GroupCharactersInc(groupId, charactersCount);
    }

    public void HandleTranslatorApiCall(long groupId, int charactersCount)
    {
        TranslatorApiCallsInc(groupId);
        TranslatorApiCharactersInc(groupId, charactersCount);
    }

    public void TranslationCacheCounterInc()
    {
        lock (_translationsCacheCounterLock)
        {
            if (++_translationsCacheCounter >= TranslatedMessagesCache.Capacity)
            {
                _translationsCacheRotations.Inc();
                _translationsCacheCounter = 0;
            }

        }
    }

    private void TotalMessagesInc() => _totalMessages.Inc();
    private void GroupMessagesInc(long groupId) => _groupMessages.WithLabels(groupId.ToString()).Inc();
    private void GroupCharactersInc(long groupId, int charactersCount) => _groupCharacters.WithLabels(groupId.ToString()).Inc(charactersCount);
    private void TranslatorApiCallsInc(long groupId) => _translatorApiCalls.WithLabels(groupId.ToString()).Inc();
    private void TranslatorApiCharactersInc(long groupId, int charactersCount) => _translatorApiCharacters.WithLabels(groupId.ToString()).Inc(charactersCount);
}