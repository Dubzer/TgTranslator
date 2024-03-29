using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TgTranslator.Data;
using TgTranslator.Models;

namespace TgTranslator.Services;

public class UsersDatabaseService
{
    private readonly TgTranslatorContext _databaseContext;

    public UsersDatabaseService(TgTranslatorContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task AddFromGroupIfNeeded(long userId)
    {
        User user = await _databaseContext.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
        if (user == null)
        {
            user = new User {PmAllowed = false, UserId = userId};
            await _databaseContext.Users.AddAsync(user);
            await _databaseContext.SaveChangesAsync();
        }
    }
        
    public async Task AddFromPmIfNeeded(long userId, string track)
    {
        User user = await _databaseContext.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
        if (user == null)
        {
            user = new User {PmAllowed = true, UserId = userId, Track = track};
            await _databaseContext.Users.AddAsync(user);
            await _databaseContext.SaveChangesAsync();
            return;
        }

        if (user.PmAllowed != true)
        {
            user.PmAllowed = true;
            await _databaseContext.SaveChangesAsync();
        }

        if (track != null && user.Track == null)
        {
            user.Track = track;
            await _databaseContext.SaveChangesAsync();
        }
    }
}