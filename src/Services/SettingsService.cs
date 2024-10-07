using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TgTranslator.Data;
using TgTranslator.Data.DTO;
using TgTranslator.Data.Options;
using TgTranslator.Menu;
using TgTranslator.Models;

namespace TgTranslator.Services;

public enum Setting
{
    Language,
    Mode
}

public class SettingsService
{
    private readonly TgTranslatorContext _databaseContext;
    private readonly SettingsValidator _settingsValidator;
    private readonly CommandsManager _commandsManager;

    public SettingsService(TgTranslatorContext databaseContext, IOptions<LanguagesList> languages, SettingsValidator settingsValidator, CommandsManager commandsManager)
    {
        _databaseContext = databaseContext;
        _settingsValidator = settingsValidator;
        _commandsManager = commandsManager;
        // TODO: Refactor it
        Static.Languages = languages.Value;
    }

    public async Task<Settings> GetSettings(long chatId)
    {
        var group = await _databaseContext.Groups.FindAsync(chatId)
                    ?? await InitSettings(chatId);

        return new Settings
        {
            TranslationMode = group.TranslationMode,
            Languages = [group.Language],
            Delay = group.Delay,
            TranslateWithLinks = group.TranslateWithLinks
        };
    }

    private async Task<Group> InitSettings(long chatId)
    {
        var group = new Group
        {
            GroupId = chatId,
            Delay = 0,
            Language = "en",
            TranslationMode = TranslationMode.Auto,
            TranslateWithLinks = true
        };

        _databaseContext.Groups.Add(group);
        await _databaseContext.SaveChangesAsync();

        return group;
    }

    public async Task SetSettings(long chatId, Settings settings)
    {
        var group = await _databaseContext.Groups.FindAsync(chatId)
                    ?? await InitSettings(chatId);

        group.Language = settings.Languages.First();
        group.Delay = settings.Delay;
        group.TranslateWithLinks = settings.TranslateWithLinks;

        _databaseContext.Update(group);
        await _databaseContext.SaveChangesAsync();

        if (group.TranslationMode != settings.TranslationMode)
            await SetMode(chatId, settings.TranslationMode);
    }

    public async Task SetLanguage(long chatId, string language)
    {
        await _databaseContext.Groups
            .Where(x => x.GroupId == chatId)
            .ExecuteUpdateAsync(x =>
                x.SetProperty(g => g.Language, language));
    }

    public async Task SetMode(long chatId, TranslationMode mode)
    {
        await _databaseContext.Groups
            .Where(x => x.GroupId == chatId)
            .ExecuteUpdateAsync(x =>
                x.SetProperty(g => g.TranslationMode, mode));

        await _commandsManager.ChangeGroupMode(chatId, mode);
    }

    public bool ValidateSettings(Setting setting, string value) =>
        setting switch
        {
            Setting.Language => _settingsValidator.ValidateLanguage(value),
            Setting.Mode => SettingsValidator.ValidateMode(value),
            _ => false
        };
}