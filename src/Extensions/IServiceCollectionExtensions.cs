using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Telegram.Bot;
using TgTranslator.Controllers;
using TgTranslator.Menu;
using TgTranslator.Models;
using TgTranslator.Services;
using TgTranslator.Services.Handlers;
using TgTranslator.Translation;
using TgTranslator.Types;
using Language = TgTranslator.Types.Language;

namespace TgTranslator.Extensions
{ 
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            var botToken = configuration.GetValue<string>("telegram:botToken");

            var yandexToken = configuration.GetValue<string>("yandex:TranslatorToken");

            services.Configure<GroupDatabaseSettings>(configuration.GetSection("MongoDB"));
            services.AddSingleton<IGroupDatabaseSettings>(provider =>
                provider.GetRequiredService<IOptions<GroupDatabaseSettings>>().Value);

            services.AddSingleton(ctx => new GroupDatabaseService(ctx.GetService<IGroupDatabaseSettings>()));
            
            services.AddSingleton(new TelegramBotClient(botToken));
            
            services.AddSingleton(ctx => 
            {
                var telegramClient = ctx.GetService<TelegramBotClient>();
                return new BotMenu(telegramClient);
            });
            
            services.AddSingleton(ctx =>
            {
                var database = ctx.GetService<GroupDatabaseService>();
                var languages = ParseLanguagesCollection("languages.json");
                return new SettingsProcessor(database, languages);
            });
            
            services.AddSingleton<ILanguageDetector>(new YandexLanguageDetector(yandexToken));
            services.AddSingleton<ITranslator>(new YandexTranslator(yandexToken));
            
            services.AddSingleton(ctx =>
            {
                var groupsBlacklist = configuration.GetSection("blacklists:groups").Get<long[]>().ToImmutableHashSet();
                var usersBlacklist = configuration.GetSection("blacklists:users").Get<int[]>().ToImmutableHashSet();
                var textsBlacklist = configuration.GetSection("blacklists:texts").Get<string[]>().ToImmutableHashSet();

                return new Blacklist(groupsBlacklist, usersBlacklist, textsBlacklist);
            });
            
            services.AddSingleton(ctx =>
            {
                var blacklist = ctx.GetService<Blacklist>();
                var charLimit = configuration.GetValue<uint>("tgtranslator:charlimit");
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
                var blacklist = ctx.GetService<Blacklist>();
                var validator = ctx.GetService<MessageValidator>();
                
                return new MessageHandler(telegramClient, botMenu, settingsProcessor, languageDetector, translator, blacklist, validator);
            });

            services.AddSingleton<ICallbackQueryHandler>(ctx => 
            {
                var botMenu = ctx.GetService<BotMenu>();

                return new CallbackQueryHandler(botMenu);
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
            string json = System.IO.File.ReadAllText(jsonFilePath);
            List<LanguageJson> langsJson = JsonConvert.DeserializeObject<List<LanguageJson>>(json);

            return langsJson.Select(language => language.Language).ToList();
        }

        public static IServiceCollection RegisterSerilog(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            return services;
        }
    }
}