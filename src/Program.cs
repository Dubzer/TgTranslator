using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry;
using Serilog;
using TgTranslator.Data.Options;
using TgTranslator.Utils.Extensions;

namespace TgTranslator;

public static class Program
{
    public static readonly DateTime StartedTime = DateTime.UtcNow;
    public static LanguagesList Languages;
    public static string Username;

    private static readonly SentryOptions SentryOptions = new()
    {
        Dsn = "https://380e0a36b482415cabd1fd621c1a030d@o797589.ingest.sentry.io/6181857",
        TracesSampleRate = 1.0
    };

    public static async Task Main()
    {
        SentrySdk.Init(SentryOptions);

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
            .ConfigureLogging(builder =>
            {
                builder.Configure(options => options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId);
            })
            .UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
                    .Ignore(new[]
                    {
                        "Microsoft.EntityFrameworkCore.Database.Command",
                        "Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker",
                        "Microsoft.AspNetCore.Hosting.Diagnostics",
                        "Microsoft.AspNetCore.Mvc.StatusCodeResult",
                        "Microsoft.EntityFrameworkCore.Infrastructure"
                    });
                loggerConfiguration.Enrich.FromLogContext();
            })
            .ConfigureAppConfiguration(config => config
                .AddJsonFile("blacklists.json")
                .AddJsonFile("languages.json"))
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseSentry(builder =>
                {
                    builder.Dsn = SentryOptions.Dsn;
                    builder.TracesSampleRate = SentryOptions.TracesSampleRate;
                });
                webBuilder.UseStartup<Startup>()
                    .UseKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")));
            });
}