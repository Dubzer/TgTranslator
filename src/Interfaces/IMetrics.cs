namespace TgTranslator.Interfaces;

public interface IMetrics
{
    void HandleGroupMessage(long groupId, int charactersCount);
    void HandleTranslatorApiCall(long groupId, int charactersCount);
    void HandleMessage();
    void HandleGroupsCountUpdate(int count);
}