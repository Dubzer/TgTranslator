using System.Collections.Generic;
using System.Globalization;
using Catglobe.ResXFileCodeGenerator;

namespace TgTranslator.Resources;

[ResxSettings(GenerateLookup = true, StaticMembers = false, MembersVisibility = Visibility.Public)]
internal partial class Localization
{
    public static IEnumerable<string> SupportedLanguages { get; } =
    [
        "", "ar", "az", "be", "de", "es", "fa", "fr", "hi", "hy", "id", "it",
        "ka", "kk", "ky", "pt", "ru", "tg", "th", "tr", "uk", "uz", "zh"
    ];

    public Localization(CultureInfo culture)
    {
        CultureInfo = culture;
    }
}
