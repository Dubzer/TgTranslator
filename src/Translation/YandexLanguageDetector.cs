using System.Threading.Tasks;
using YandexTranslateCSharpSdk;

namespace TgTranslator.Translation
{
    public class YandexLanguageDetector : ILanguageDetector
    {
        readonly YandexTranslateSdk _yaTranslator = new YandexTranslateSdk();

        public YandexLanguageDetector(string apiKey)
        {
            _yaTranslator.ApiKey = apiKey;
        }

        public async Task<string> DetectLanguageAsync(string text)
        {
            return await _yaTranslator.DetectLanguage(text);
        }
    }
}