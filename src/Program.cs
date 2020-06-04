using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using TgTranslator.Data.Options;

namespace TgTranslator
{
    public static class Program
    {
        public static LanguagesList Languages;

        public static async Task Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += (sender, eventArgs) => Console.WriteLine(eventArgs.ExceptionObject);

            await CreateHostBuilder()
                .Build()
                .RunAsync();
        }

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration))
                .ConfigureAppConfiguration(config => config
                    .AddJsonFile("blacklists.json")
                    .AddJsonFile("languages.json"))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")));
                });
    }
}