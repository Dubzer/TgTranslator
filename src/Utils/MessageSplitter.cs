using System;

namespace TgTranslator.Utils;

public static class MessageSplitter
{
    private static readonly char[] SplitChars = ['.', '?', '!', ','];

    public static (string firstPart, string secondPart) Split(ReadOnlySpan<char> message)
    {
        // We add 1 because we want char to be included in the first part of the translation
        // Hence we don't include 4096th char
        var lastEndOfSentence = message[..4096].LastIndexOfAny(SplitChars) + 1;

        // Hard trim
        if(lastEndOfSentence == 0)
            lastEndOfSentence = 4096;

        return (message[..lastEndOfSentence].ToString(), message[lastEndOfSentence..].ToString());
    }
}