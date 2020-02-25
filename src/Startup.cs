using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using TgTranslator.Extensions;

namespace TgTranslator
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.RegisterSerilog(_configuration);
            services.RegisterServices(_configuration);
            services.AddMvc(options => { options.EnableEndpointRouting = false; }).AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseHttpsRedirection();
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