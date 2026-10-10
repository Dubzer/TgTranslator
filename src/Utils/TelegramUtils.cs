#nullable enable
using Telegram.Bot.Types;

namespace TgTranslator.Utils;

public static class TelegramUtils
{
    public static readonly LinkPreviewOptions DisabledLinkPreview = new()
    {
        IsDisabled = true
    };

    public static ReplyParameters SafeReplyTo(MessageId messageId) => new()
    {
        MessageId = messageId,
        AllowSendingWithoutReply = false
    };

    public static (ReplyParameters ReplyParameters, EphemeralMessageParameters? EphemeralParameters)
        OptionalEphemeralReply(Message message, bool botIsAdministrator = false)
    {
        ReplyParameters replyParameters;
        if (message.EphemeralMessageId != null)
            replyParameters = new ReplyParameters { EphemeralMessageId = message.EphemeralMessageId };
        else
            replyParameters = SafeReplyTo(message.MessageId);

        if (message.From is not { IsBot: false }
            || (message.EphemeralMessageId == null && !botIsAdministrator))
            return (replyParameters, null);

        var ephemeralParameters = new EphemeralMessageParameters { ReceiverUserId = message.From.Id };
        return (replyParameters, ephemeralParameters);
    }
}
