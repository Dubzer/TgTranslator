using System.Text.RegularExpressions;

namespace TgTranslator.Utils.Extensions;

public static partial class StringExtensions
{
    // This regex searches for codepoints that are considered category of letters in Unicode
    [GeneratedRegex(@"\p{L}")]
    private static partial Regex LettersRegex();

    public static bool AnyLetters(this string value) => LettersRegex().IsMatch(value);
}