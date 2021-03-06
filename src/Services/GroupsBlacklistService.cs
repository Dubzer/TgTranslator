using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TgTranslator.Data;
using TgTranslator.Data.Options;
using TgTranslator.Models;

namespace TgTranslator.Services
{
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
            Group group = await _database.Groups.Where(g => g.GroupId == groupId).Include(g => g.GroupBlacklist).FirstOrDefaultAsync();
            if (group.GroupBlacklist != null)
            {
                group.GroupBlacklist.AddedAt = DateTime.UtcNow;
                await _database.SaveChangesAsync();
                return;
            }
                    
            await _database.GroupsBlacklist.AddAsync(new GroupBlacklist {GroupId = groupId});
            await _database.SaveChangesAsync();
        }

        public async Task<bool> InBlacklist(long groupId)
        {
            Group group = await _database.Groups.Where(g => g.GroupId == groupId).Include(g => g.GroupBlacklist).FirstOrDefaultAsync();
            if (group?.GroupBlacklist == null)
                return false;

            if (IsBanExpired(group.GroupBlacklist.AddedAt))
            {
                _database.Remove(group.GroupBlacklist);
                await _database.SaveChangesAsync();
                return false;
            }

            return true;
        }

        private bool IsBanExpired(DateTime addedAt) => addedAt.ToUniversalTime() + _blacklistTime < DateTime.UtcNow;
    }    
}