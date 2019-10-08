using System.Threading.Tasks;

namespace TgTranslator.Translation
{
    public interface ITranslator
    {
        Task<string> TranslateTextAsync(string text, string from, string to);
    }
}