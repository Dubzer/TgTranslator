using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgTranslator.Services.EventHandlers;

public class MyChatMemberHandler
{
    private readonly GroupsBlacklistService _blacklistService;

    public MyChatMemberHandler(GroupsBlacklistService blacklistService)
    {
        _blacklistService = blacklistService;
    }

    public async Task Handle(ChatMemberUpdated update)
    {
        if (update.Chat.Type is not (ChatType.Group or ChatType.Supergroup))
           return;

        if (update.NewChatMember.User.Username != Static.Username)
            return;
        
        if (update.NewChatMember.Status is ChatMemberStatus.Restricted && !((ChatMemberRestricted)update.NewChatMember).CanSendMessages)
        {
            await _blacklistService.AddGroup(update.Chat.Id);
            return;
        }
        
        await _blacklistService.RemoveGroup(update.Chat.Id);
    }
}