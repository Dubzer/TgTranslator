using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Data.Options;
using TgTranslator.Extensions;

namespace TgTranslator.Validation;

public class MessageValidator
{
    private readonly Blacklists _blacklist;
    private readonly uint _charLimit;

    public MessageValidator(IOptionsSnapshot<Blacklists> blacklistsOptions, IOptions<TgTranslatorOptions> tgTranslatorOptions)
    {
        _blacklist = blacklistsOptions.Value;
        _charLimit = tgTranslatorOptions.Value.CharLimit;
    }

    public bool GroupMessageValid(Message message, string messageText) =>
        !_blacklist.GroupIdsBlacklist.Contains(message.Chat.Id)
        && messageText.Length > 1
        && messageText.Length <= _charLimit
        && !messageText.StartsWith('.')
        && !_blacklist.TextsBlacklist.Contains(messageText.ToLowerInvariant())
        && (messageText == $"@{Program.Username}" || !message.IsOnlyLinks());
}