#nullable enable
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgTranslator.Utils;

public static class TranslationUtils
{
    public static string FixEntities(string messageText, string translation, MessageEntity[]? entities)
    {
        if (entities == null || entities.Length == 0)
            return translation;

        var usernames = entities
            .Where(x => x is { Type: MessageEntityType.Mention })
            .Select(x => messageText[(x.Offset + 1)..(x.Offset + x.Length)])
            .ToArray();

        if (usernames.Length == 0)
            return translation;

        translation = usernames.Aggregate(translation, (current, username) => current.Replace($"@ {username}", $"@{username}"));
        return translation;
    }
}