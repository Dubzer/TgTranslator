using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TgTranslator.Data.Options;
using TgTranslator.Interfaces;
using TgTranslator.Menu;
using TgTranslator.Services;
using TgTranslator.Services.Handlers;
using TgTranslator.Services.Translation;
using TgTranslator.Stats;
using TgTranslator.Utils.Extensions;
using TgTranslator.Validation;

namespace TgTranslator;

public static class DiServices
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        var telegramOptions = configuration.GetSection("telegram").Get<TelegramOptions>();

        services.AddTransient<GroupDatabaseService>();
        services.AddTransient<UsersDatabaseService>();
        services.AddTransient<GroupsBlacklistService>();

        services.AddSingleton(new TelegramBotClient(telegramOptions.BotToken));

        services.AddSingleton<IMetrics, Metrics>();
        services.AddTransient<BotMenu>();

        services.AddTransient<ILanguageDetector, TranslatePlaceholderService>();
        services.AddTransient<ITranslator, TranslatePlaceholderService>();

        services.AddTransient<MessageValidator>();
        services.AddTransient<SettingsProcessor>();

        services.AddTransient<IMessageHandler, MessageHandler>();
        services.AddTransient<ICallbackQueryHandler, CallbackQueryHandler>();
        services.AddTransient<MyChatMemberHandler>();

        services.AddTransient<IpWhitelist>();
        
        if (telegramOptions.Webhooks)
            //  Register webhooks.
            //  IStartupFilter calls the service only once, after building the DI container, but before the app starts receiving messages 
            services.AddTransient<IStartupFilter, TelegramWebhooksRegistrar>();
        else
            //  Receive events using polling
            //  TelegramBotHostedService basically wraps Telegram.Bot lib's events into Controller 
            services.AddHostedService<TelegramBotHostedService>();   

        return services;
    }
}
