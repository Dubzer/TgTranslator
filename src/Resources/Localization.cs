using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using Catglobe.ResXFileCodeGenerator;

namespace TgTranslator.Resources;

[ResxSettings(GenerateLookup = true, StaticMembers = false, MembersVisibility = Visibility.Public)]
internal partial class Localization
{
    private static readonly FrozenDictionary<string, string> ResourceLanguages = new Dictionary<string, string>()
    {
        [""] = "",
        ["zh"] = "zh",
        ["fa"] = "fa",
        ["ru"] = "ru",
        ["ar"] = "ar",
        ["id"] = "id",
        ["fr"] = "fr",
        ["es"] = "es",
        ["be"] = "ru",
        ["uk"] = "ru",
        ["kk"] = "ru",
        ["ky"] = "ru",
        ["tg"] = "ru",
        ["hy"] = "ru",
        ["uz"] = "ru",
        ["ka"] = "ru"
    }.ToFrozenDictionary();

    public static IEnumerable<string> SupportedLanguages => ResourceLanguages.Keys;

    public Localization(CultureInfo culture)
    {
        CultureInfo = culture;
        var languageCode = culture.TwoLetterISOLanguageName;

        if (ResourceLanguages.TryGetValue(languageCode, out var resourceLanguage)
            && resourceLanguage != languageCode)
        {
            CultureInfo = CultureInfo.GetCultureInfo(resourceLanguage);
        }
    }
}
