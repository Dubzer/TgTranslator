using System.Threading.Tasks;

namespace TgTranslator.Interfaces
{
    public interface ITranslator
    {
        Task<string> TranslateTextAsync(string text, string from, string to);
    }
}