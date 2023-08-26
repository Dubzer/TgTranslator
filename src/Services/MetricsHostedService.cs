using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TgTranslator.Data;
using TgTranslator.Stats;

namespace TgTranslator.Services;

public class MetricsHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Metrics _metrics;
    private readonly PeriodicTimer _timer;

    public MetricsHostedService(IServiceScopeFactory scopeFactory, Metrics metrics)
    {
        _scopeFactory = scopeFactory;
        _metrics = metrics;
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetService<TgTranslatorContext>();

                var groupsCount = await context.Groups.CountAsync(cancellationToken);
                _metrics.TotalGroups.Set(groupsCount);

                var usersCount = await context.Users.CountAsync(cancellationToken);
                _metrics.TotalUsers.Set(usersCount);

                var pmUsersCount = await context.Users.Where(x => x.PmAllowed).CountAsync(cancellationToken);
                _metrics.TotalPmUsers.Set(pmUsersCount);

                await _timer.WaitForNextTickAsync(cancellationToken);
            }
        }, TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}