using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Telegram.Bot;
using TgTranslator.Controllers;
using TgTranslator.Data;
using TgTranslator.Menu;
using TgTranslator.Models;
using TgTranslator.Services;
using TgTranslator.Services.Handlers;
using TgTranslator.Stats;
using TgTranslator.Translation;

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
            services.AddSingleton(ctx =>
            {
                var telegramClient = ctx.GetService<TelegramBotClient>();
                string caption = configuration.GetValue<string>("helpmenu:videourl");
                return new BotMenu(telegramClient, caption);
            });

            services.AddSingleton(ctx =>
            {
                var database = ctx.GetService<GroupDatabaseService>();
                List<Language> languages = ParseLanguagesCollection("languages.json");
                return new SettingsProcessor(database, languages);
            });

            services.AddSingleton<ILanguageDetector>(new YandexLanguageDetector(yandexToken));
            services.AddSingleton<ITranslator>(new YandexTranslator(yandexToken));
            services.AddSingleton(ctx =>
            {
                ImmutableHashSet<long> groupsBlacklist = configuration.GetSection("blacklists:groups").Get<long[]>().ToImmutableHashSet();
                ImmutableHashSet<int> usersBlacklist = configuration.GetSection("blacklists:users").Get<int[]>().ToImmutableHashSet();
                ImmutableHashSet<string> textsBlacklist = configuration.GetSection("blacklists:texts").Get<string[]>().ToImmutableHashSet();

                return new Blacklist(groupsBlacklist, usersBlacklist, textsBlacklist);
            });

            services.AddSingleton(ctx =>
            {
                var blacklist = ctx.GetService<Blacklist>();
                uint charLimit = configuration.GetValue<uint>("tgtranslator:charlimit");
                return new MessageValidator(blacklist, charLimit);
            });

            #region Handlers

            services.AddSingleton<IMessageHandler>(ctx =>
            {
                var telegramClient = ctx.GetService<TelegramBotClient>();
                var botMenu = ctx.GetService<BotMenu>();
                var settingsProcessor = ctx.GetService<SettingsProcessor>();
                var languageDetector = ctx.GetService<ILanguageDetector>();
                var translator = ctx.GetService<ITranslator>();
                var metrics = ctx.GetService<IMetrics>();
                var blacklist = ctx.GetService<Blacklist>();
                var validator = ctx.GetService<MessageValidator>();

                return new MessageHandler(telegramClient, botMenu, settingsProcessor, languageDetector, translator, metrics, blacklist, validator);
            });

            services.AddSingleton<ICallbackQueryHandler>(ctx =>
            {
                var botMenu = ctx.GetService<BotMenu>();
                var telegramClient = ctx.GetService<TelegramBotClient>();

                return new CallbackQueryHandler(botMenu, telegramClient);
            });

            #endregion

            services.AddSingleton(ctx =>
            {
                var telegramClient = ctx.GetService<TelegramBotClient>();
                var messageHandler = ctx.GetService<IMessageHandler>();
                var callbackQueryHandler = ctx.GetService<ICallbackQueryHandler>();

                return new TelegramBotController(telegramClient, messageHandler, callbackQueryHandler);
            });
            services.AddHostedService<TelegramBotHostedService>();
            return services;
        }

        private static List<Language> ParseLanguagesCollection(string jsonFilePath)
        {
            string json = File.ReadAllText(jsonFilePath);
            var langsJson = JsonConvert.DeserializeObject<List<LanguageJson>>(json);

            return langsJson.Select(language => language.Language).ToList();
        }

        public static IServiceCollection RegisterSerilog(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            return services;
        }
    }
}