using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgTranslator.Extensions
{
    public static class TelegramExtensions
    {
        public static bool IsOnlyLinks(this Message message)
        {
            if (message.Entities == null)
                return false;

            IEnumerable<MessageEntity> linksArray = message.Entities.Where(e => e.Type == MessageEntityType.Url
                                                                                || e.Type == MessageEntityType.Mention
                                                                                || e.Type == MessageEntityType.Cashtag
                                                                                || e.Type == MessageEntityType.Email
                                                                                || e.Type == MessageEntityType.PhoneNumber
                                                                                || e.Type == MessageEntityType.Hashtag);

            string withoutLinks = linksArray.Reverse()
                .Aggregate(message.Text, (current, e) => current.Remove(e.Offset, e.Length));

            return !withoutLinks.Any(char.IsLetterOrDigit);
        }

        
        /// <summary>
        /// Checks if user is an administrator
        /// </summary>
        public static async Task<bool> IsAdministrator(this User user, long chatId, TelegramBotClient client)
        {
            ChatMember[] chatAdmins = await client.GetChatAdministratorsAsync(chatId);

            return chatAdmins.Any(x => x.User.Id == user.Id);
        }

        public static bool IsCommand(this Message message) => 
            message.Entities?.Length == 1 && message.Entities[0].Type == MessageEntityType.BotCommand;
    }
}