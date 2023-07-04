using System;

namespace TgTranslator.Utils;

public static class MessageSplitter
{
    private static readonly char[] SplitChars = {'.', '?', '!', ','};

    public static (string, string) Split(ReadOnlySpan<char> message)
    {
        // We use 4095 instead of 4096 because we want char to be included in the first part
        var lastEndOfSentence = message[..4096].LastIndexOfAny(SplitChars) + 1;

        // Hard trim
        if(lastEndOfSentence == 0)
            lastEndOfSentence = 4096;

        var firstPart = message[..lastEndOfSentence];
        var secondPart = message[lastEndOfSentence..];

        return (firstPart.ToString(), secondPart.ToString());
    }
}