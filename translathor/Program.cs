using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Translathor
{
    class Program
    {
        public static ITelegramBotClient botClient;

        public static IConfigurationRoot Configuration { get; set; }

        static async void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("configuration.json");
            Configuration = builder.Build();
            LoggingService.PrepareDirectory();

            LoggingService.Log("Starting up...");
            botClient = new TelegramBotClient(Configuration["tokens:telegramapi"]);

            botClient.StartReceiving();
            LoggingService.Log("Receiving messages...");
            UpdateHandler updateHandler = new UpdateHandler();
            botClient.OnMessage += updateHandler.Bot_OnMessage;

            await Task.Delay(-1);

        }
    }
}
