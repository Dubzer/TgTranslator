using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using TgTranslator.Types;
using Newtonsoft.Json;
using TgTranslator.Extensions;

namespace TgTranslator
{
    public class Program
    {
        public static List<Language> languages;

        public static async Task Main(string[] args)
        {
            // TODO: move to builder
            languages = ParseLanguagesCollection(AppDomain.CurrentDomain.BaseDirectory + "languages.json");

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("blacklists.json", false);
            
            var configuration = builder.Build();
            var polling = configuration.GetValue<bool>("telegram:polling");
            if (polling)
            {
                await new HostBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.RegisterSerilog(configuration);
                        services.RegisterServices(configuration);
                    })
                    .RunConsoleAsync();
            }
            else
            {
                await WebHost.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration(config => { config.AddJsonFile("blacklists.json");})
                    .UseStartup<Startup>()
                    .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration))
                    .Build()
                    .RunAsync();
            }
        }

        private static List<Language> ParseLanguagesCollection(string jsonFilePath)
        {
            string json = System.IO.File.ReadAllText(jsonFilePath);
            List<LanguageJson> langsJson = JsonConvert.DeserializeObject<List<LanguageJson>>(json);

            return langsJson.Select(language => language.Language).ToList();
        }
    }
}
