using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TgTranslator.Data;
using TgTranslator.Data.DTO;
using TgTranslator.Data.Options;
using TgTranslator.Menu;

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

    public SettingsService(TgTranslatorContext databaseContext, IOptions<LanguagesList> languages, SettingsValidator settingsValidator)
    {
        _databaseContext = databaseContext;
        _settingsValidator = settingsValidator;
        // TODO: Refactor it
        Static.Languages = languages.Value;
    }

    public async Task<Settings> GetSettings(long chatId)
    {
        var group = await _databaseContext.Groups.FindAsync(chatId);
        ArgumentNullException.ThrowIfNull(group);

        return new Settings
        {
            TranslationMode = group.TranslationMode,
            Languages = [group.Language],
            Delay = group.Delay
        };
    }

    public async Task SetSettings(long chatId, Settings settings)
    {
        var group = await _databaseContext.Groups.FindAsync(chatId);
        ArgumentNullException.ThrowIfNull(group);

        group.Language = settings.Languages.First();
        group.TranslationMode = settings.TranslationMode;
        group.Delay = settings.Delay;

        _databaseContext.Update(group);
        await _databaseContext.SaveChangesAsync();
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
    }

    public bool ValidateSettings(Setting setting, string value) =>
        setting switch
        {
            Setting.Language => _settingsValidator.ValidateLanguage(value),
            Setting.Mode => SettingsValidator.ValidateMode(value),
            _ => false
        };
}