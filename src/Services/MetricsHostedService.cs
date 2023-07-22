using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TgTranslator.Data;
using TgTranslator.Interfaces;
using TgTranslator.Stats;

namespace TgTranslator.Services;

public class MetricsHostedService : IHostedService
{
    private readonly TgTranslatorContext _context;
    private readonly IMetrics _metrics;
    private readonly PeriodicTimer _timer;

    public MetricsHostedService(IMetrics metrics, TgTranslatorContext context)
    {
        _context = context;
        _metrics = metrics;
        _timer = new PeriodicTimer(TimeSpan.FromHours(3));
    }

    // ReSharper disable once FunctionNeverReturns
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var numberOfGroups = await _context.Groups.CountAsync(cancellationToken);
            _metrics.HandleGroupsCountUpdate(numberOfGroups);
            await _timer.WaitForNextTickAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}