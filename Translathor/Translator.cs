using Extensions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YandexTranslateCSharpSdk;

namespace Translathor
{
    class Translator
    {
        YandexTranslateSdk yaTranslator = new YandexTranslateSdk();

        public Translator(string apiKey)
        {
            yaTranslator.ApiKey = apiKey;
        }

        public async Task<string> DetectLanguage(string text)
        {
            // Text without English symbols
            string textWOEng = Regex.Replace(text.WithoutLinks(), @"[A-Za-z0-9 .,-=@+(){}\[\]\\]", "");

            if (!string.IsNullOrWhiteSpace(textWOEng))
            {
                return await yaTranslator.DetectLanguage(textWOEng);
            }

            return await yaTranslator.DetectLanguage(text.WithoutLinks());
        }

        public async Task<string> TranslateText(string text, string from, string to)
        {
            string translation = await yaTranslator.TranslateText(text, $"{from}-{to}");
            LoggingService.Log($"Translated {text} ({from}) to {translation} ({to})");

            return translation;
        }
    }
}
