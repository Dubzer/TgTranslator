using System.Threading.Tasks;

namespace TgTranslator.Translation
{
    public interface ILanguageDetector
    {
        Task<string> DetectLanguageAsync(string text);
    }
}