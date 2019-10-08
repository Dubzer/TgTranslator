using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgTranslator.Extensions
{
    public static class TelegramExtensions
    {
        public static bool IsLink(this Message message)
        {
            if (message.Entities == null)
                return false;
            
            return message.Entities[0].Type == MessageEntityType.Url && message.Entities[0].Length == message.Text.Length;
        }

        public static bool IsHashtag(this Message message)
        {
            if (message.Entities == null)
                return false;
            
            return message.Entities[0].Type == MessageEntityType.Hashtag && message.Entities[0].Length == message.Text.Length;
        }

        public static bool IsCommand(this Message message)
        {
            if (message.Entities == null)
                return false;

            return message.Entities[0].Type == MessageEntityType.BotCommand;
        }

        /// <summary>
        /// Checks if user is an administrator
        /// </summary>
        public static async Task<bool> IsAdministrator(this User user, long chatId, TelegramBotClient client)
        {
            ChatMember[] chatAdmins = await client.GetChatAdministratorsAsync(chatId);

            return chatAdmins.Any(x => x.User.Id == user.Id);
        }
    }
}