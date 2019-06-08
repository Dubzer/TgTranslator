using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TgTranslator
{
    class Program
    {
        public static ITelegramBotClient botClient;

        public static IConfigurationRoot Configuration { get; set; }

        static async Task Main()
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("configuration.json");
            Configuration = builder.Build();
            LoggingService.PrepareDirectory();

            LoggingService.Log("Starting up...");
            botClient = new TelegramBotClient(Configuration["tokens:telegramapi"]);

            botClient.OnMessage += UpdateHandler.Bot_OnMessage;
            botClient.OnCallbackQuery += BotClient_OnCallbackQuery;
            botClient.StartReceiving();
            LoggingService.Log("Receiving messages...");
            await Task.Delay(-1);

        }
    }
}
