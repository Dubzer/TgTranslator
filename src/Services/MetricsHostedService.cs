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

    public MetricsHostedService(IMetrics metrics, TgTranslatorContext context)
    {
        _context = context;
        _metrics = metrics;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var numberOfGroups = await _context.Groups.CountAsync(cancellationToken);
            _metrics.HandleGroupsCountUpdate(numberOfGroups);
            await Task.Delay(TimeSpan.FromHours(3), cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}