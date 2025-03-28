using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using TgTranslator.Data;
using TgTranslator.Data.DTO;
using TgTranslator.Data.Options;
using TgTranslator.Interfaces;
using TgTranslator.Menu;
using TgTranslator.Services;
using TgTranslator.Services.EventHandlers;
using TgTranslator.Services.EventHandlers.Messages;
using TgTranslator.Services.Translation;
using TgTranslator.Stats;
using TgTranslator.Utils.Extensions;
using TgTranslator.Validation;

namespace TgTranslator;

public static class DiServices
{
    public static IHostApplicationBuilder RegisterServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<TgTranslatorContext>(b =>
            b.UseNpgsql(builder.Configuration.GetConnectionString("TgTranslatorContext")));

        builder.Services.AddCors();
        builder.Services.ConfigureTelegramBotMvc();

        var telegramOptions = builder.Configuration.GetSection("telegram").Get<TelegramOptions>();

        builder.Services.AddTransient<UsersDatabaseService>();
        builder.Services.AddTransient<GroupsBlacklistService>();

        builder.Services.AddSingleton(new TelegramBotClient(telegramOptions.BotToken));

        builder.Services.AddSingleton<Metrics>();
        builder.Services.AddSingleton<TranslatedMessagesCache>();
        builder.Services.AddSingleton<LanguagesProvider>();

        builder.Services.AddTransient<BotMenu>();

        builder.Services.AddSingleton<ITranslator, TranslatePlaceholderService>();

        builder.Services.AddTransient<MessageValidator>();
        builder.Services.AddTransient<SettingsService>();
        builder.Services.AddTransient<CommandsManager>();
        builder.Services.AddSingleton<SettingsValidator>();

        builder.Services.AddTransient<ICallbackQueryHandler, CallbackQueryHandler>();
        builder.Services.AddTransient<MyChatMemberHandler>();
        builder.Services.AddTransient<EditedMessageHandler>();

        builder.Services.AddTransient<MessageRouter>();
        builder.Services.AddTransient<TranslateHandler>();
        builder.Services.AddTransient<CommandHandler>();
        builder.Services.AddTransient<SettingsChangeHandler>();

        builder.Services.AddTransient<IpWhitelist>();
        builder.Services.AddTransient<EventRouter>();

        builder.Services.AddHostedService<MetricsHostedService>();

        builder.Services.AddSingleton<WebAppHashService>();

        if (telegramOptions.Webhooks)
            //  Register webhooks.
            //  IStartupFilter calls the service only once, after building the DI container, but before the app starts receiving messages 
            builder.Services.AddTransient<IStartupFilter, TelegramWebhooksRegistrar>();
        else
            //  Receive events using polling
            //  TelegramBotHostedService basically wraps Telegram.Bot lib's events into Controller 
            builder.Services.AddHostedService<TelegramBotHostedService>();

        if (builder.Environment.IsDevelopment())
            builder.Services.AddTransient<IStartupFilter, DatabaseMigrator>();

        return builder;
    }
}
