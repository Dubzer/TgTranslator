#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgTranslator.Utils.Extensions;

public static class TelegramExtensions
{
    extension(Message message)
    {
        public bool IsOnlyEntities()
        {
            var entities = message.TextOrCaptionEntities;
            var text = message.TextOrCaption;

            if (entities == null || text == null)
                return false;

            var entitiesArray = entities
                .Where(e => e.Type 
                    is MessageEntityType.Url 
                    or MessageEntityType.Mention 
                    or MessageEntityType.Cashtag 
                    or MessageEntityType.Email 
                    or MessageEntityType.PhoneNumber 
                    or MessageEntityType.Hashtag
                    or MessageEntityType.Pre
                    or MessageEntityType.Code);
            
            var withoutLinks = entitiesArray.Reverse().Aggregate(text, (current, e) => current.Remove(e.Offset, e.Length));
            return !withoutLinks.Any(char.IsLetterOrDigit);
        }

        public string? TextOrCaption => message.Text ?? message.Caption;
        public MessageEntity[]? TextOrCaptionEntities => message.Entities ?? message.CaptionEntities;

        public bool IsCommand => 
            message is { Entities: [{ Type: MessageEntityType.BotCommand }], Text: not null };
    }

    extension(User user)
    {
        /// <summary>
        /// Checks if user is an administrator
        /// </summary>
        public async Task<bool> IsAdministrator(long chatId, TelegramBotClient client)
        {
            if (user.IsAnonymousAdmin)
                return true;
            
            var chatAdmins = await client.GetChatAdministrators(chatId);
            return chatAdmins.Any(x => x.User.Id == user.Id);
        }

        public bool IsAnonymousAdmin => user.Id == 1087968824;
    }
}