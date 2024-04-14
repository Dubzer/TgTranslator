using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TgTranslator.Data;

namespace TgTranslator.Services;

public class DatabaseMigrator : IStartupFilter
{
    /// <summary>
    ///
    /// </summary>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        builder =>
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TgTranslatorContext>();
                db.Database.Migrate();
            }

            next(builder);
        };
}