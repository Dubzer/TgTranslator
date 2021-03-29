using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using TgTranslator.Data.Options;
using TgTranslator.Extensions;

namespace TgTranslator
{
    public static class Program
    {
        public static readonly DateTime StartedTime = DateTime.UtcNow;
        public static LanguagesList Languages;

        public static async Task Main()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) => Log.Fatal(eventArgs.ExceptionObject.ToString());

            await CreateHostBuilder()
                .Build()
                .RunAsync();
        }

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
                    .Ignore(new []
                    {
                        "Microsoft.EntityFrameworkCore.Database.Command",
                        "Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker",
                        "Microsoft.AspNetCore.Hosting.Diagnostics",
                        "Microsoft.AspNetCore.Mvc.StatusCodeResult",
                        "Microsoft.EntityFrameworkCore.Infrastructure"
                    }))
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