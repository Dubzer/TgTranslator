using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using TgTranslator.Types;

namespace TgTranslator
 {
     public class SettingsProcessor
     {
         private readonly IDatabase _database;
         private readonly List<Language> _languages;

         public SettingsProcessor(IDatabase database, List<Language> languages)
         {
            _database = database;
            _languages = languages;
         }
         
         public string GetGroupLanguage(long chatId)
         {
             var key = _database.StringGet($"{chatId}:lang");

             if (key.IsNullOrEmpty)
             {
                 ChangeSetting(chatId, "lang", "en");
                 return "en";
             }

             return key;
         }

         public void ChangeSetting(long chatId, string param, string value)
         {
            _database.StringSet($"{chatId}:{param}", value);
         }

         public bool ValidateLanguage(string languageCode)
         {
             return _languages.Any(x => x.Code == languageCode);
         }
     }
 } 