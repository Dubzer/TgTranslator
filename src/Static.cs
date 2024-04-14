using System;
using TgTranslator.Data.Options;

namespace TgTranslator;

// Should not exist
public static class Static
{
    public static readonly DateTime StartedTime = DateTime.UtcNow;
    public static LanguagesList Languages;
    public static string Username;
    public static long BotId;
}