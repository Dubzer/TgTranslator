using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgTranslator
{
    class Program
    {
        public static ITelegramBotClient botClient;

        public static IConfigurationRoot Configuration { get; set; }

        private static async Task Main()
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("configuration.json");
            Configuration = builder.Build();
            LoggingService.PrepareDirectory();

            LoggingService.Log("Starting up...");
            botClient = new TelegramBotClient(Configuration["tokens:telegramapi"]);

            botClient.OnMessage += async (sender, args) => { await new UpdateHandler().OnMessage(args); };
            botClient.OnCallbackQuery += async (sender, args) => { await new UpdateHandler().OnCallbackQuery(args); };
            botClient.StartReceiving();
            LoggingService.Log("Receiving messages...");
            await Task.Delay(-1);
        }
    }
}
