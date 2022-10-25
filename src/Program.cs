using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Sentry;
using Serilog;
using TgTranslator.Data.Options;
using TgTranslator.Extensions;

namespace TgTranslator;

public static class Program
{
    public static readonly DateTime StartedTime = DateTime.UtcNow;
    public static LanguagesList Languages;
    public static string Username;
    public static async Task Main()
    {
        SentrySdk.Init(o =>
        {
            o.Dsn = "https://380e0a36b482415cabd1fd621c1a030d@o797589.ingest.sentry.io/6181857";
            // When configuring for the first time, to see what the SDK is doing:
            o.Debug = false;
            // Set traces_sample_rate to 1.0 to capture 100% of transactions for performance monitoring.
            // We recommend adjusting this value in production.
            o.TracesSampleRate = 1.0;
            o.AddDiagnosticSourceIntegration();
        });

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) => Log.Fatal(eventArgs.ExceptionObject as Exception, "Something went terribly wrong");

        await CreateHostBuilder()
            .Build()
            .RunAsync();
        
        SentrySdk.Close();
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