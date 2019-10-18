using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Extensions;

namespace TgTranslator
{
    public class MessageValidator
    {
        private readonly uint _charLimit;
        private readonly Blacklist _blacklist;

        public MessageValidator(Blacklist blacklist, uint charLimit)
        {
            _blacklist = blacklist;
            _charLimit = charLimit;
        }
        
        public bool GroupMessageValid(Message message)
        {
            return !_blacklist.IsGroupBlocked(message.Chat.Id)
                   && message.Type == MessageType.Text
                   && message.Text.Length <= _charLimit
                   && _blacklist.IsTextAllowed(message.Text)
                   && !message.IsLink()
                   && !message.IsHashtag()
                   && !message.IsOnlyMention();
        }
    }
}