using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgTranslator.Extensions;

public static class TelegramExtensions
{
    public static bool IsOnlyLinks(this Message message)
    {
        if (message.Entities == null)
            return false;

        IEnumerable<MessageEntity> linksArray = message.TextOrCaptionEntities()
            .Where(e => e.Type 
                is MessageEntityType.Url 
                or MessageEntityType.Mention 
                or MessageEntityType.Cashtag 
                or MessageEntityType.Email 
                or MessageEntityType.PhoneNumber 
                or MessageEntityType.Hashtag);

            
        string withoutLinks = linksArray.Reverse()
            .Aggregate(message.TextOrCaption(), (current, e) => current.Remove(e.Offset, e.Length));

        return !withoutLinks.Any(char.IsLetterOrDigit);
    }

    public static string TextOrCaption(this Message message)
    {
        return message.Text ?? message.Caption;
    }

    public static MessageEntity[] TextOrCaptionEntities(this Message message)
    {
        return message.Entities ?? message.CaptionEntities;
    }
        
    /// <summary>
    /// Checks if user is an administrator
    /// </summary>
    public static async Task<bool> IsAdministrator(this User user, long chatId, TelegramBotClient client)
    {
        ChatMember[] chatAdmins = await client.GetChatAdministratorsAsync(chatId);

        //                                                  Todo: replace with something better when telegram lib will update
        return chatAdmins.Any(x => x.User.Id == user.Id) || user.Id == 1087968824;
    }

    public static bool IsCommand(this Message message) => 
        message.Entities?.Length == 1 && message.Entities[0].Type == MessageEntityType.BotCommand;
}