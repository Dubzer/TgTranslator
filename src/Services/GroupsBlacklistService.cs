using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TgTranslator.Data;
using TgTranslator.Models;

namespace TgTranslator.Services
{
    public class GroupsBlacklistService
    {
        private readonly TgTranslatorContext _database;
        private readonly TimeSpan _blacklistTime;
        
        public GroupsBlacklistService(TgTranslatorContext database)
        {
            _database = database;
            _blacklistTime = TimeSpan.FromHours(3);
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

            if (group.GroupBlacklist.AddedAt + _blacklistTime < DateTime.UtcNow)
            {
                _database.Remove(group.GroupBlacklist);
                await _database.SaveChangesAsync();
                return false;
            }

            return true;
        }
    }
}