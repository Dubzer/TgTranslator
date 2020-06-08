using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using TgTranslator.Data.Options;
using TgTranslator.Extensions;
using TgTranslator.Services.Middlewares;

namespace TgTranslator
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
 
        public Startup(IConfiguration configuration) => _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.RegisterServices(_configuration);
            services.AddMvc(options => { options.EnableEndpointRouting = false; })
                .AddControllersAsServices()
                .AddNewtonsoftJson();
            
            services.Configure<KestrelServerOptions>(_configuration.GetSection("Kestrel"));
            services.Configure<LanguagesList>(_configuration.GetSection("languages"));
            services.Configure<TgTranslatorOptions>(_configuration.GetSection("TgTranslator"));
            services.Configure<Blacklists>(_configuration.GetSection("blacklists"));
            services.Configure<HelpmenuOptions>(_configuration.GetSection("helpmenu"));
            services.Configure<YandexOptions>(_configuration.GetSection("yandex"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
            app.Map("/metrics", metricsApp =>
            {
                metricsApp.UseMiddleware<BasicAuthMiddleware>(_configuration.GetValue<string>("prometheus:login"),
                    _configuration.GetValue<string>("prometheus:password"));
                metricsApp.UseMetricServer("");
            });
        }
    }
}