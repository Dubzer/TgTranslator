using System.Threading.Tasks;
using Serilog;
using YandexTranslateCSharpSdk;

namespace TgTranslator.Translation
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
            Log.Information($"Translated [ {text} ] ({from}) to [ {translation} ] ({to})");

            return translation;
        }

    }
}