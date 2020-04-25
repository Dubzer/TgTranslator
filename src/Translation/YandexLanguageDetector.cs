using System.Threading.Tasks;
using TgTranslator.Interfaces;
using YandexTranslateCSharpSdk;

namespace TgTranslator.Translation
{
    public class YandexLanguageDetector : ILanguageDetector
    {
        private readonly YandexTranslateSdk _yaTranslator = new YandexTranslateSdk();

        public YandexLanguageDetector(string apiKey) => _yaTranslator.ApiKey = apiKey;

        #region ILanguageDetector Members

        public async Task<string> DetectLanguageAsync(string text) => await _yaTranslator.DetectLanguage(text);

        #endregion
    }
}