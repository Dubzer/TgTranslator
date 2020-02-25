using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Extensions;

namespace TgTranslator
{
    public class MessageValidator
    {
        private readonly Blacklist _blacklist;
        private readonly uint _charLimit;

        public MessageValidator(Blacklist blacklist, uint charLimit)
        {
            _blacklist = blacklist;
            _charLimit = charLimit;
        }

        public bool GroupMessageValid(Message message) =>
            !_blacklist.IsGroupBlocked(message.Chat.Id)
            && message.Type == MessageType.Text
            && message.Text.Length <= _charLimit
            && _blacklist.IsTextAllowed(message.Text)
            && !message.IsOnlyLinks();
    }
}