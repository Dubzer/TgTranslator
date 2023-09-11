using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using TgTranslator.Data;
using TgTranslator.Data.Options;
using TgTranslator.Services.Middlewares;

namespace TgTranslator;

public class Startup
{
    private readonly IConfiguration _configuration;
    public Startup(IConfiguration configuration) => _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TgTranslatorContext>(builder =>
            builder.UseNpgsql(_configuration.GetConnectionString("TgTranslatorContext")));
        services.RegisterServices(_configuration);
        services.AddCors();
        services.AddMvc(options => { options.EnableEndpointRouting = false; })
            .AddControllersAsServices()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });

        services.Configure<KestrelServerOptions>(_configuration.GetSection("Kestrel"));
        services.Configure<LanguagesList>(_configuration.GetSection("languages"));
        services.Configure<TgTranslatorOptions>(_configuration.GetSection("TgTranslator"));
        services.Configure<Blacklists>(_configuration.GetSection("blacklists"));
        services.Configure<HelpmenuOptions>(_configuration.GetSection("helpmenu"));
        services.Configure<TelegramOptions>(_configuration.GetSection("telegram"));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
        if (env.IsDevelopment())
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
        app.UseMvcWithDefaultRoute();
        app.UseWhen(
            ctx => ctx.Request.Path.StartsWithSegments("/api/bot"),
            ab => ab.UseMiddleware<EnableRequestBodyBufferingMiddleware>()
        );
        app.Map("/metrics", metricsApp =>
        {
            metricsApp.UseMiddleware<BasicAuthMiddleware>(_configuration.GetValue<string>("prometheus:login"),
                _configuration.GetValue<string>("prometheus:password"));
            metricsApp.UseMetricServer("");
        });
    }
}