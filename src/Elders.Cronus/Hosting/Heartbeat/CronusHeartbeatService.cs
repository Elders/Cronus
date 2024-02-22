using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Hosting.Heartbeat;

public sealed class CronusHeartbeatService : BackgroundService
{
    private readonly ILogger<CronusHeartbeatService> _logger;

    public CronusHeartbeatService(IServiceProvider services, ILogger<CronusHeartbeatService> logger)
    {
        Services = services;
        _logger = logger;
    }

    public IServiceProvider Services { get; }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Consume Scoped Service Hosted Service running.");

        return DoWork(stoppingToken);
    }

    private Task DoWork(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service is working.");

            var heartbeat = Services.GetRequiredService<IHeartbeat>();
            return heartbeat.StartBeatingAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.ErrorException(ex, () => "Failed to send heartbeat.");
            return Task.FromException(ex);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Consume Scoped Service Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}
