using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TgTranslator.Data.Options;
using TgTranslator.Interfaces;
using YandexTranslateCSharpSdk;

namespace TgTranslator.Translation
{
    public class YandexTranslator : ITranslator
    {
        private readonly YandexTranslateSdk _yaTranslator = new YandexTranslateSdk();

        public YandexTranslator(IOptions<YandexOptions> options)
        {
            _yaTranslator.ApiKey = options.Value.TranslatorToken;
        }

        public Task<string> TranslateTextAsync(string text, string to)
        {
            return _yaTranslator.TranslateTextAsync(text, to);
        }
    }
}