using System.Linq;

namespace TgTranslator.Utils;

public static class MessageSplitter
{
    private static readonly char[] SplitChars = {'.', '?', '!', ','};

    public static (string, string) Split(string message)
    {
        // We use 4095 instead of 4096 because we want char to be included in the first part
        var lastEndOfSentence = SplitChars.Select(x => FindLastIndexWithinLimit(message, x, 4095)).Max() + 1;

        // Hard trim
        if(lastEndOfSentence == 0)
            lastEndOfSentence = 4096;

        var firstPart = message[..lastEndOfSentence];
        var secondPart = message[lastEndOfSentence..];

        return (firstPart, secondPart);
    }

    private static int FindLastIndexWithinLimit(string message, char splitChar, int limit)
    {
        for (var i = message.Length - 1; i >= 0; i--)
        {
            if (message[i] == splitChar && i <= limit)
                return i;
        }

        return -1;
    }
}