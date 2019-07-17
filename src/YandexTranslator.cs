using System.Threading.Tasks;
using TgTranslator.Interfaces;
using YandexTranslateCSharpSdk;

namespace TgTranslator
{
    public class YandexTranslator : ITranslator
    {
        YandexTranslateSdk yaTranslator = new YandexTranslateSdk();

        public YandexTranslator(string apiKey)
        {
            yaTranslator.ApiKey = apiKey;
        }
        
        public async Task<string> TranslateTextAsync(string text, string from, string to)
        {
            string translation = await yaTranslator.TranslateText(text, $"{from}-{to}");
            LoggingService.Log($"Translated [ {text} ] ({from}) to [ {translation} ] ({to})");

            return translation;
        }

    }
}