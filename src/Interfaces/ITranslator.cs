using System.Threading.Tasks;

namespace TgTranslator.Interfaces;

public record TranslationResult(string Text, string DetectedLanguage);

public interface ITranslator
{
    Task<TranslationResult> TranslateTextAsync(string text, string to);
}