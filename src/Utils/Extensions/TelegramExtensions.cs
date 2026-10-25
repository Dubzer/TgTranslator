#nullable enable
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgTranslator.Utils.Extensions;

public static class TelegramExtensions
{
    private static readonly FrozenDictionary<string, int> MessageToEditNotFoundErrors = new Dictionary<string, int>
    {
        ["Bad Request: message to edit not found"] = 400,
        ["Bad Request: message not found"] = 400,
        ["Bad Request: MESSAGE_ID_INVALID"] = 400,
        ["Bad Request: MSG_ID_INVALID"] = 400
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, int> MessageNotFoundErrors = new Dictionary<string, int>
    {
        ["Bad Request: message not found"] = 400,
        ["Bad Request: message to be replied not found"] = 400,
        ["Bad Request: REPLY_MESSAGE_ID_INVALID"] = 400
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, int> ChatPermissionDeniedErrors = new Dictionary<string, int>
    {
        ["Bad Request: CHAT_RESTRICTED"] = 400,
        ["Bad Request: CHAT_WRITE_FORBIDDEN"] = 400,
        ["Bad Request: CHAT_SEND_PLAIN_FORBIDDEN"] = 400,
        ["Bad Request: USER_BANNED_IN_CHANNEL"] = 400,
        ["Bad Request: CHANNEL_PRIVATE"] = 400,
        ["Bad Request: CHAT_GUEST_SEND_FORBIDDEN"] = 400,
        ["Bad Request: have no write access to the chat"] = 400,
        ["Bad Request: have no rights to send a message"] = 400,
        ["Bad Request: not enough rights to send text messages to the chat"] = 400,
        ["Bad Request: not enough rights to send the rich message to the chat"] = 400,
        ["Forbidden: the group chat was deleted"] = 403,
        ["Forbidden: bot was kicked from the group chat"] = 403,
        ["Forbidden: bot was kicked from the supergroup chat"] = 403,
        ["Forbidden: bot was kicked from the channel chat"] = 403,
        ["Forbidden: bot is not a member of the group chat"] = 403,
        ["Forbidden: bot is not a member of the supergroup chat"] = 403,
        ["Forbidden: bot is not a member of the channel chat"] = 403
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    extension(ApiRequestException exception)
    {
        public bool IsMessageToEditNotFound() => MatchesError(exception, MessageToEditNotFoundErrors);

        public bool IsMessageNotFound() => MatchesError(exception, MessageNotFoundErrors);

        public bool IsChatPermissionDenied() => MatchesError(exception, ChatPermissionDeniedErrors);
    }

    private static bool MatchesError(ApiRequestException exception, FrozenDictionary<string, int> errors) =>
        errors.TryGetValue(exception.Message, out var errorCode) && exception.ErrorCode == errorCode;

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
