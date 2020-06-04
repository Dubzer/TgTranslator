using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TgTranslator.Data.Options;
using TgTranslator.Interfaces;
using YandexTranslateCSharpSdk;

namespace TgTranslator.Translation
{
    public class YandexLanguageDetector : ILanguageDetector
    {
        private readonly YandexTranslateSdk _translator = new YandexTranslateSdk();

        public YandexLanguageDetector(IOptions<YandexOptions> options)
        {
            _translator.ApiKey = options.Value.TranslatorToken;
        }
        
        public Task<string> DetectLanguageAsync(string text)
        {
            return _translator.DetectLanguageAsync(text);
        }
    }
}