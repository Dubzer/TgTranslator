using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using TgTranslator.Data.Options;
using TgTranslator.Utils.Extensions;

namespace TgTranslator.Validation;

public class MessageValidator
{
    private readonly Blacklists _blacklist;

    public MessageValidator(IOptionsSnapshot<Blacklists> blacklistsOptions)
    {
        _blacklist = blacklistsOptions.Value;
    }

    public bool GroupMessageValid(Message message, string messageText) =>
        !_blacklist.GroupIdsBlacklist.Contains(message.Chat.Id)
        && messageText.Length > 1
        && !messageText.StartsWith('.')
        && !_blacklist.TextsBlacklist.Contains(messageText.ToLowerInvariant())
        && (messageText == $"@{Static.Username}" || !message.IsOnlyEntities());
}