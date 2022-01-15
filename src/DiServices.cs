using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TgTranslator.Data.Options;
using TgTranslator.Extensions;
using TgTranslator.Interfaces;
using TgTranslator.Menu;
using TgTranslator.Services;
using TgTranslator.Services.Handlers;
using TgTranslator.Stats;
using TgTranslator.Validation;

namespace TgTranslator;

public static class DiServices
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        var telegramOptions = configuration.GetSection("telegram").Get<TelegramOptions>();

            
        services.AddScoped<GroupDatabaseService>();
        services.AddScoped<UsersDatabaseService>();
        services.AddScoped<GroupsBlacklistService>();
            
        services.AddSingleton(new TelegramBotClient(telegramOptions.BotToken));

        services.AddSingleton<IMetrics, Metrics>();
        services.AddScoped<BotMenu>();
            
        //services.AddTransient<ILanguageDetector, YandexLanguageDetector>();
        //services.AddTransient<ITranslator, YandexTranslator>();
        services.AddScoped<ILanguageDetector, TranslatorMicroservice>();
        services.AddSingleton<ITranslator, TranslatorMicroservice>();

        services.AddScoped<MessageValidator>();
        services.AddScoped<SettingsProcessor>();
            
        services.AddScoped<IMessageHandler, MessageHandler>();
        services.AddScoped<ICallbackQueryHandler, CallbackQueryHandler>();

        services.AddScoped<IpWhitelist>();
        
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