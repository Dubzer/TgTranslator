using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace TgTranslator
{
    class Program
    {
        public static ITelegramBotClient botClient;
        public static IConfigurationRoot Configuration { get; set; }
        
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
            botClient.StartReceiving();
            LoggingService.Log("Receiving messages...");
            await Task.Delay(-1);
        }
    }
}
