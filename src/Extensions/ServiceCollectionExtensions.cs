using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TgTranslator.Data;
using TgTranslator.Data.Options;
using TgTranslator.Interfaces;
using TgTranslator.Menu;
using TgTranslator.Services;
using TgTranslator.Services.Handlers;
using TgTranslator.Stats;
using TgTranslator.Validation;

namespace TgTranslator.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            var telegramOptions = configuration.GetSection("telegram").Get<TelegramOptions>();

            services.AddDbContext<TgTranslatorContext>(builder => builder.UseNpgsql(configuration.GetConnectionString("TgTranslatorContext")));
            
            services.AddTransient<GroupDatabaseService>();
            services.AddTransient<UsersDatabaseService>();
            services.AddTransient<GroupsBlacklistService>();
            
            services.AddSingleton(new TelegramBotClient(telegramOptions.BotToken));

            services.AddSingleton<IMetrics, Metrics>();
            services.AddTransient<BotMenu>();
            
            services.AddTransient<ILanguageDetector, YandexLanguageDetector>();
            services.AddTransient<ITranslator, YandexTranslator>();

            services.AddTransient<MessageValidator>();
            services.AddTransient<SettingsProcessor>();
            
            services.AddTransient<IMessageHandler, MessageHandler>();
            services.AddTransient<ICallbackQueryHandler, CallbackQueryHandler>();
            
            if (telegramOptions.Webhooks)
                //  Register webhooks.
                //  IStartupFilter calls the service only once, after building the DI container, but before the app starts receiving messages 
                services.AddTransient<IStartupFilter, TelegramWebhooksExtensions>();    
            else
                //  Receive events using polling
                //  TelegramBotHostedService basically wraps Telegram.Bot lib's events into Controller 
                services.AddHostedService<TelegramBotHostedService>();   

            return services;
        }
    }
}