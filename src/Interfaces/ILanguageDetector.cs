using System.Threading.Tasks;

namespace TgTranslator.Interfaces
{
    public interface ILanguageDetector
    {
        Task<string> DetectLanguageAsync(string text);
    }
}