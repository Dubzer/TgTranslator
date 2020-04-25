using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TgTranslator.Controllers;
using TgTranslator.Data;
using TgTranslator.Interfaces;
using TgTranslator.Menu;
using TgTranslator.Services;
using TgTranslator.Services.Handlers;
using TgTranslator.Stats;
using TgTranslator.Translation;
using TgTranslator.Validation;

namespace TgTranslator.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            string botToken = configuration.GetValue<string>("telegram:botToken");

            string yandexToken = configuration.GetValue<string>("yandex:TranslatorToken");

            services.AddDbContext<TgTranslatorContext>(builder => builder
                .UseNpgsql(configuration.GetConnectionString("TgTranslatorContext")), ServiceLifetime.Singleton);
            
            services.AddSingleton<GroupDatabaseService>();

            services.AddSingleton(new TelegramBotClient(botToken));

            services.AddSingleton<IMetrics>(new Metrics());
            services.AddSingleton<BotMenu>();
            
            services.AddSingleton<ILanguageDetector>(new YandexLanguageDetector(yandexToken));
            services.AddSingleton<ITranslator>(new YandexTranslator(yandexToken));

            services.AddSingleton<MessageValidator>();
            services.AddSingleton<SettingsProcessor>();
            
            services.AddSingleton<IMessageHandler, MessageHandler>();
            services.AddSingleton<ICallbackQueryHandler, CallbackQueryHandler>();
            
            services.AddSingleton<TelegramBotController>();
            services.AddHostedService<TelegramBotHostedService>();
            return services;
        }
    }
}