using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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

public class SettingsProcessor
{
    private readonly GroupDatabaseService _database;
    private readonly LanguagesList _languages;

    public SettingsProcessor(GroupDatabaseService database, IOptions<LanguagesList> languages)
    {
        _database = database;
        _languages = languages.Value;
        // TODO: Refactor it
        Program.Languages = languages.Value;
    }

    public async Task<Settings> GetGroupConfiguration(long chatId)
    {
        var group = await _database.GetAsync(chatId);
        return new()
        {
            TranslationMode = group.TranslationMode,
            Languages = new[] { group.Language }
        };
    }

    public async Task<string> GetGroupLanguage(long chatId)
    {
        Group group = await _database.GetAsync(chatId);
        return group.Language;
    }

    public async Task<TranslationMode> GetTranslationMode(long chatId)
    {
        Group group = await _database.GetAsync(chatId);
        return group.TranslationMode;
    }

    public async Task ChangeLanguage(long chatId, string language)
    {
        Group group = await _database.GetAsync(chatId);
        if (group.Language == language)
            return;
            
        await _database.UpdateLanguageAsync(group, language);
    }

    public async Task ChangeMode(long chatId, TranslationMode mode)
    {
        Group group = await _database.GetAsync(chatId);
        if (group.TranslationMode == mode)
            return;
            
        await _database.UpdateModeAsync(group, mode);
    }

    public bool ValidateSettings(Setting setting, string value) =>
        setting switch
        {
            Setting.Language => _languages.Select(l => l.Code).Contains(value),
            Setting.Mode => Enum.TryParse(typeof(TranslationMode), value, true, out _),
            _ => false
        };
}