using System.Threading.Tasks;
using YandexTranslateCSharpSdk;

namespace TgTranslator.Translation
{
    public class YandexTranslator : ITranslator
    {
        private readonly YandexTranslateSdk _yaTranslator = new YandexTranslateSdk();

        public YandexTranslator(string apiKey) => _yaTranslator.ApiKey = apiKey;

        #region ITranslator Members

        public async Task<string> TranslateTextAsync(string text, string to) => await _yaTranslator.TranslateText(text, to);

        #endregion
    }
}