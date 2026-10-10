using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using Serilog;
using TgTranslator;
using TgTranslator.Data.Options;
using TgTranslator.Menu;
using TgTranslator.Services.Middlewares;
using TgTranslator.Stats;
using TgTranslator.Utils.Extensions;

_ = Static.StartedTime;  // Initialize startup time

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) => Log.Fatal(eventArgs.ExceptionObject as Exception, "Something went terribly wrong");

var builder = WebApplication.CreateBuilder(args);


builder.Configuration
    .AddJsonFile("blacklists.json")
    .AddJsonFile("languages.json")
    .AddJsonFile("appsettings.local.json", optional: true);

builder
    .ConfigureOptionFromSection<KestrelServerOptions>("Kestrel")
    .ConfigureOptionFromSection<LanguagesList>("languages")
    .ConfigureOptionFromSection<TgTranslatorOptions>("TgTranslator")
    .ConfigureOptionFromSection<Blacklists>("Blacklists")
    .ConfigureOptionFromSection<TelegramOptions>("Telegram");


builder.Logging.Configure(options => options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId);
builder.WebHost.UseSentry(options =>
{
    options.Dsn = "https://380e0a36b482415cabd1fd621c1a030d@o797589.ingest.sentry.io/6181857";
    options.TracesSampleRate = 1.0;
});

builder.Services.AddSerilog((_, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(builder.Configuration)
        .Ignore([
            "Microsoft.EntityFrameworkCore.Database.Command",
            "Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker",
            "Microsoft.AspNetCore.Hosting.Diagnostics",
            "Microsoft.AspNetCore.Mvc.StatusCodeResult",
            "Microsoft.EntityFrameworkCore.Infrastructure",
            "Microsoft.AspNetCore.Routing.EndpointMiddleware"
        ]);

    loggerConfig.Enrich.FromLogContext();
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter<TranslationMode>(JsonNamingPolicy.CamelCase));
    });

builder
    .AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter(Metrics.MeterName, "System.Net.Http")
        .AddView("http.client.request.duration", new ExplicitBucketHistogramConfiguration
        {
            Boundaries = [0.01, 0.015625, 0.03125, 0.0625, 0.125, 0.25, 0.5, 1, 2, 5, 10, 30]
        })
        .AddView("translation_response_time_ms", new ExplicitBucketHistogramConfiguration
        {
            Boundaries = [250, 500, 750, 1000, 1250, 1500, 1750, 3000, 5000, 8000]
        })
        .AddPrometheusExporter(options => options.DisableTotalNameSuffixForCounters = true));

builder.RegisterServices();


var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseCors(x => x
        .AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(_ => true)
        .AllowCredentials());
}
else
{
    app.UseCors(x => x
        .WithMethods("GET", "PUT", "OPTIONS", "HEAD")
        .AllowAnyHeader()
        .WithOrigins("https://tgtrns.dubzer.dev")
        .AllowCredentials());
}

app.MapControllers();
app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/api/bot"),
    ab => ab.UseMiddleware<EnableRequestBodyBufferingMiddleware>()
);
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/metrics"),
    metricsApp => metricsApp.UseMiddleware<BasicAuthMiddleware>(
        builder.Configuration.GetValue<string>("prometheus:login"),
        builder.Configuration.GetValue<string>("prometheus:password")));
app.MapPrometheusScrapingEndpoint();


await app.RunAsync();
