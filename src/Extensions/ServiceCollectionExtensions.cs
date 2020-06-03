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

            services.AddDbContext<TgTranslatorContext>(builder => builder
                .UseNpgsql(configuration.GetConnectionString("TgTranslatorContext")));
            
            services.AddTransient<GroupDatabaseService>();

            services.AddSingleton(new TelegramBotClient(botToken));

            services.AddSingleton<IMetrics>(new Metrics());
            services.AddSingleton<BotMenu>();
            
            services.AddSingleton<ILanguageDetector, YandexLanguageDetector>();
            services.AddSingleton<ITranslator, YandexTranslator>();

            services.AddSingleton<MessageValidator>();
            services.AddTransient<SettingsProcessor>();
            
            services.AddTransient<IMessageHandler, MessageHandler>();
            services.AddSingleton<ICallbackQueryHandler, CallbackQueryHandler>();
            
            
            services.AddHostedService<TelegramBotHostedService>();
            return services;
        }
    }
}