using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using TgTranslator.Extensions;
using TgTranslator.Types;

namespace TgTranslator
{
    public static class Program
    {
        public static List<Language> languages;

        public static async Task Main(string[] args)
        {
            // TODO: move to builder
            languages = ParseLanguagesCollection(AppDomain.CurrentDomain.BaseDirectory + "languages.json");

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("blacklists.json", false);

            IConfigurationRoot configuration = builder.Build();
            bool polling = configuration.GetValue<bool>("telegram:polling");

            if (polling)
                await new HostBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.RegisterSerilog(configuration);
                        services.RegisterServices(configuration);
                    })
                    .RunConsoleAsync();
            else
                await WebHost.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration(config => { config.AddJsonFile("blacklists.json"); })
                    .UseStartup<Startup>()
                    .UseKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")))
                    .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration))
                    .Build()
                    .RunAsync();
        }

        private static List<Language> ParseLanguagesCollection(string jsonFilePath)
        {
            string json = File.ReadAllText(jsonFilePath);
            var langsJson = JsonConvert.DeserializeObject<List<LanguageJson>>(json);

            return langsJson.Select(language => language.Language).ToList();
        }
    }
}