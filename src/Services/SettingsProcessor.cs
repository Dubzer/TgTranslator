using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TgTranslator.Menu;
using TgTranslator.Models;

namespace TgTranslator.Services
{
    public enum Setting
    {
        Language,
        Mode
    }

    public class SettingsProcessor
    {
        private readonly GroupDatabaseService _database;
        private readonly List<Language> _languages;

        public SettingsProcessor(GroupDatabaseService database, List<Language> languages)
        {
            _database = database;
            _languages = languages;
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
}