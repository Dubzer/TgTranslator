using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using TgTranslator.Types;

namespace TgTranslator
 {
     public class SettingsProcessor
     {
         private readonly IDatabase database;

         public SettingsProcessor()
         {
             var redis = ConnectionMultiplexer.Connect(Program.Configuration["tokens:redisip"]);
             database = redis.GetDatabase();
         }
         
         public string GetGroupLanguage(long chatId)
         {
             var key = database.StringGet($"{chatId}:lang");

             if (key.IsNullOrEmpty)
             {
                 ChangeSetting(chatId, "lang", "en");
                 return "en";
             }

             return key;
         }

         public void ChangeSetting(long chatId, string param, string value)
         {
             database.StringSet($"{chatId}:{param}", value);
         }

         public bool ValidateLanguage(string languageCode, IEnumerable<Language> languagesList)
         {
             return languagesList.Any(x => x.Code == languageCode);
         }
     }
 } 