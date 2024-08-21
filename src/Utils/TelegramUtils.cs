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
}