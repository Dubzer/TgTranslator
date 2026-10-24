using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TgTranslator.Data;
using TgTranslator.Data.Options;
using TgTranslator.Menu;
using TgTranslator.Models;

namespace TgTranslator.Services;

public class GroupsBlacklistService
{
    private readonly TgTranslatorContext _database;
    private readonly TimeSpan _blacklistTime;
        
    public GroupsBlacklistService(TgTranslatorContext database, IOptions<TgTranslatorOptions> tgTranslatorOptions)
    {
        _database = database;
        _blacklistTime = TimeSpan.FromMinutes(tgTranslatorOptions.Value.BanTime);
    }
        
    public async Task AddGroup(long groupId)
    {
        await _database.Groups.Upsert(new Group
            {
                GroupId = groupId,
                Languages = ["en"],
                TranslationMode = TranslationMode.Auto,
                Delay = 0,
                TranslateWithLinks = true
            })
            .On(group => group.GroupId)
            .NoUpdate()
            .RunAsync();

        await _database.GroupsBlacklist.Upsert(new GroupBlacklist { GroupId = groupId, AddedAt = DateTime.UtcNow })
            .On(blacklist => blacklist.GroupId)
            .WhenMatched((existing, incoming) => new GroupBlacklist { AddedAt = incoming.AddedAt })
            .RunAsync();
    }

    public async Task RemoveGroup(long groupId)
    {
        var group = await _database.Groups
            .Where(g => g.GroupId == groupId)
            .Include(g => g.GroupBlacklist)
            .FirstOrDefaultAsync();
        
        if (group?.GroupBlacklist == null)
            return;
                    
        _database.GroupsBlacklist.Remove(group.GroupBlacklist);
        await _database.SaveChangesAsync();
    }
    
    public async Task<bool> InBlacklist(long groupId)
    {
        Group group = await _database.Groups.Where(g => g.GroupId == groupId).Include(g => g.GroupBlacklist).FirstOrDefaultAsync();
        if (group?.GroupBlacklist == null)
            return false;

        if (!IsBanExpired(@group.GroupBlacklist.AddedAt))
            return true;
            
        _database.Remove(@group.GroupBlacklist);
        await _database.SaveChangesAsync();
        return false;

    }

    private bool IsBanExpired(DateTime addedAt) => addedAt.ToUniversalTime() + _blacklistTime < DateTime.UtcNow;
}
