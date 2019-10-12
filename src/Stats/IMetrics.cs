namespace TgTranslator.Stats
{
    public interface IMetrics
    {
        void HandleGroupMessage(long groupId, int charactersCount);
        void HandleTranslatorApiCall(long groupId, int charactersCount);
        void HandleMessage();
    }
}