using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using TgTranslator.Menu;
using TgTranslator.Models;
using TgTranslator.Services;
using TgTranslator.Types;

namespace TgTranslator
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
        private readonly ImmutableDictionary<string, TranslationMode> _modes;

        public SettingsProcessor(GroupDatabaseService database, List<Language> languages)
        {
            _database = database;
            _languages = languages;

            _modes = new Dictionary<string, TranslationMode>
            {
                {"auto", TranslationMode.Auto},
                {"forwards", TranslationMode.Forwards},
                {"manual", TranslationMode.Manual}
            }.ToImmutableDictionary();
        }

        public async Task<string> GetGroupLanguage(long chatId)
        {
            Group group = await _database.Get(chatId);
            return group.Language;
        }

        public async Task<TranslationMode> GetTranslationMode(long chatId)
        {
            Group group = await _database.Get(chatId);
            return group.Mode;
        }

        public async Task ChangeLanguage(long chatId, string language) => await _database.UpdateLanguage(_database.Get(chatId).Result, language);

        public async Task ChangeMode(long chatId, TranslationMode mode)
        {
            Group group = await _database.Get(chatId);
            await _database.UpdateMode(group, mode);
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