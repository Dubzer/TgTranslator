using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Data.Options;
using TgTranslator.Extensions;

namespace TgTranslator.Validation
{
    public class MessageValidator
    {
        private readonly Blacklists _blacklist;
        private readonly uint _charLimit;

        public MessageValidator(IOptionsSnapshot<Blacklists> blacklistsOptions, IOptions<TgTranslatorOptions> tgTranslatorOptions)
        {
            _blacklist = blacklistsOptions.Value;
            _charLimit = tgTranslatorOptions.Value.CharLimit;
        }

        public bool GroupMessageValid(Message message) =>
            !_blacklist.GroupIdsBlacklist.Contains(message.Chat.Id)
            && message.Type == MessageType.Text
            && message.Text.Length <= _charLimit
            && !_blacklist.TextsBlacklist.Contains(message.Text)
            && !message.IsOnlyLinks()
            && message.Text[0] != '.';
    }
}