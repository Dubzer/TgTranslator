using System.Data;
using StackExchange.Redis;

namespace TgTranslator
 {
     public class SettingsProcessor
     {
         private ConnectionMultiplexer redis;
         private IDatabase db;

         public SettingsProcessor()
         {
            redis = ConnectionMultiplexer.Connect("127.0.0.1"); 
            db = redis.GetDatabase();
         }
         
         public string GetGroupLanguage(long chatId)
         {
             var key = db.StringGet($"{chatId}:lang");

             if (!key.IsNullOrEmpty)
             {
                 return key;
             }
             else
             {
                 ChangeSetting(chatId, "lang", "en");
                 return "en";
             }
         }

         public void ChangeSetting(long chatId, string param, string value)
         {
             db.StringSet($"{chatId}:{param}", value);
         }
     }
 } 