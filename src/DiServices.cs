using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using TgTranslator.Data;
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
    public static IHostApplicationBuilder RegisterServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<TgTranslatorContext>(b =>
            b.UseNpgsql(builder.Configuration.GetConnectionString("TgTranslatorContext")));

        builder.Services.AddCors();
        builder.Services.AddMvc(options => { options.EnableEndpointRouting = false; })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });

        var telegramOptions = builder.Configuration.GetSection("telegram").Get<TelegramOptions>();

        builder.Services.AddTransient<GroupDatabaseService>();
        builder.Services.AddTransient<UsersDatabaseService>();
        builder.Services.AddTransient<GroupsBlacklistService>();

        builder.Services.AddSingleton(new TelegramBotClient(telegramOptions.BotToken));

        builder.Services.AddSingleton<Metrics>();
        builder.Services.AddTransient<BotMenu>();

        builder.Services.AddTransient<ITranslator, TranslatePlaceholderService>();

        builder.Services.AddTransient<MessageValidator>();
        builder.Services.AddTransient<SettingsProcessor>();

        builder.Services.AddTransient<IMessageHandler, MessageHandler>();
        builder.Services.AddTransient<ICallbackQueryHandler, CallbackQueryHandler>();
        builder.Services.AddTransient<MyChatMemberHandler>();

        builder.Services.AddTransient<IpWhitelist>();
        builder.Services.AddTransient<HandlersRouter>();

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
