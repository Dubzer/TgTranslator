using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using TgTranslator.Types;

namespace TgTranslator
{
    class Program
    {
        public static ITelegramBotClient botClient;
        public static IConfigurationRoot Configuration { get; set; }

        public static List<Language> languages;
        private static async Task Main()
        {
            UpdateHandler updateHandler = new UpdateHandler();
            
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("configuration.json");
            Configuration = builder.Build();
            LoggingService.PrepareDirectory();

            LoggingService.Log("Starting up...");
            botClient = new TelegramBotClient(Configuration["tokens:telegramapi"]);

            botClient.OnMessage += async (sender, args) => { await updateHandler.OnMessage(args); };
            botClient.OnCallbackQuery += async (sender, args) => { await updateHandler.OnCallbackQuery(args); };
            
            LoggingService.Log("Parsing languages list...");
            languages = ParseLanguagesCollection("languages.json");

            botClient.StartReceiving();
            LoggingService.Log("Receiving messages...");
            await Task.Delay(-1);
        }
        
        private static List<Language> ParseLanguagesCollection(string jsonFilePath)
        {
            List<Language> result = new List<Language>();
            
            string json = System.IO.File.ReadAllText(jsonFilePath);
            List<LanguageJson> langsJson = JsonConvert.DeserializeObject<List<LanguageJson>>(json);
            foreach (var language in langsJson)
            {
                result.Add(language.Language);
            }

            return result;
        }
    }
}
