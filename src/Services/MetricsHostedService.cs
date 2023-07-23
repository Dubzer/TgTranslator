using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TgTranslator.Data;
using TgTranslator.Interfaces;
using TgTranslator.Stats;

namespace TgTranslator.Services;

public class MetricsHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMetrics _metrics;
    private readonly PeriodicTimer _timer;

    public MetricsHostedService(IServiceScopeFactory scopeFactory, IMetrics metrics)
    {
        _scopeFactory = scopeFactory;
        _metrics = metrics;
        _timer = new PeriodicTimer(TimeSpan.FromHours(3));
    }

    // ReSharper disable once FunctionNeverReturns
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetService<TgTranslatorContext>();
                var numberOfGroups = await context.Groups.CountAsync(cancellationToken);
                _metrics.HandleGroupsCountUpdate(numberOfGroups);
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