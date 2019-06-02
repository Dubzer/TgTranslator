using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace translathor
{
    class Translathor
    {
        public static ITelegramBotClient botClient;

        public static IConfigurationRoot Configuration { get; set; }

        public Translathor(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("configuration.json");
            Configuration = builder.Build();
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Translathor(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
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
