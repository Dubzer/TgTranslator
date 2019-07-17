using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Extensions;
using TgTranslator.Interfaces;
using YandexTranslateCSharpSdk;

namespace TgTranslator
{
    public class YandexLanguageDetector : ILanguageDetector
    {
        YandexTranslateSdk yaTranslator = new YandexTranslateSdk();

        public YandexLanguageDetector(string apiKey)
        {
            yaTranslator.ApiKey = apiKey;
        }

        public async Task<string> DetectLanguageAsync(string text)
        {
            // Text without English symbols
            string textWOEng = Regex.Replace(text.WithoutLinks(), "[a-zA-Z0-9 -]", "");

            // The second thing counts percentage of non-English symbols, and if it's > 8%, then translation is not required
            if (!string.IsNullOrWhiteSpace(textWOEng) && textWOEng.Length * 100 / text.Length > 8)
            {
                return await yaTranslator.DetectLanguage(textWOEng);
            }

            return await yaTranslator.DetectLanguage(text.WithoutLinks());

        }
    }
}