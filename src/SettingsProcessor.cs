using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TgTranslator.Models;
using TgTranslator.Services;
using TgTranslator.Types;

namespace TgTranslator
 {
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
             var group = await _database.Get(chatId);
             
             if (group == null)
                 return _database.Create(new Group(chatId)).Result.Language;

             return group.Language; 
         }

         public async Task ChangeLanguage(long chatId, string language) =>
            await _database.UpdateLanguage(_database.Get(chatId).Result, language);

         public bool ValidateLanguage(string languageCode) => _languages.Any(x => x.Code == languageCode);
     }
 } 