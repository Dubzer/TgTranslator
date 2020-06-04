using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
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
            services.AddTransient<UsersDatabaseService>();
            
            services.AddSingleton(new TelegramBotClient(botToken));

            services.AddSingleton<IMetrics, Metrics>();
            services.AddTransient<BotMenu>();
            
            services.AddTransient<ILanguageDetector, YandexLanguageDetector>();
            services.AddTransient<ITranslator, YandexTranslator>();

            services.AddTransient<MessageValidator>();
            services.AddTransient<SettingsProcessor>();
            
            services.AddTransient<IMessageHandler, MessageHandler>();
            services.AddTransient<ICallbackQueryHandler, CallbackQueryHandler>();
            
            
            services.AddHostedService<TelegramBotHostedService>();
            return services;
        }
    }
}