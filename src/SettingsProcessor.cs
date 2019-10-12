using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
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
         
         public string GetGroupLanguage(long chatId)
         {
             var group = _database.Get(chatId);
             
             if (group == null)
                 return _database.Create(new Group(chatId)).Language;

             return group.Language; 
         }

         public void ChangeLanguage(long chatId, string language)
         {
            _database.UpdateLanguage(_database.Get(chatId), language);
         }

         public bool ValidateLanguage(string languageCode)
         {
             return _languages.Any(x => x.Code == languageCode);
         }
     }
 } 