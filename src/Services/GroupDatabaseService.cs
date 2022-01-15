using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TgTranslator.Data;
using TgTranslator.Menu;
using TgTranslator.Models;

namespace TgTranslator.Services;

public class GroupDatabaseService
{
    private readonly TgTranslatorContext _databaseContext;
        
    public GroupDatabaseService(TgTranslatorContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<Group> GetAsync(long chatId)
    {
        Group group = await _databaseContext.Groups
            .Where(grp => grp.GroupId == chatId)
            .FirstOrDefaultAsync() ?? await AddAsync(new Group { GroupId = chatId });
            
        return group;
    }

    private async Task<Group> AddAsync(Group group)
    {
        await _databaseContext.AddAsync(group);
        await _databaseContext.SaveChangesAsync();
        return group;
    }

    public async Task UpdateLanguageAsync(Group groupIn, string language)
    {
        groupIn.Language = language;
        _databaseContext.Update(groupIn);
        await _databaseContext.SaveChangesAsync();
    }

    public async Task UpdateModeAsync(Group groupIn, TranslationMode mode)
    {
        groupIn.TranslationMode = mode;
        _databaseContext.Update(groupIn);
        await _databaseContext.SaveChangesAsync();
    }
}