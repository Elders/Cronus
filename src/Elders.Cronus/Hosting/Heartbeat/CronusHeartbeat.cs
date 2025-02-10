using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Hosting.Heartbeat;

public class CronusHeartbeat : IHeartbeat
{
    private readonly IPublisher<ISignal> publisher;
    private readonly BoundedContext boundedContext;
    private readonly ILogger<CronusHeartbeat> logger;

    private const string TTL = "5000";

    private TenantsOptions tenants;
    private HeartbeatOptions options;

    public CronusHeartbeat(IPublisher<ISignal> publisher, IOptionsMonitor<BoundedContext> boundedContext, IOptionsMonitor<HeartbeatOptions> heartbeatOptions, IOptionsMonitor<TenantsOptions> tenantsOptions, ILogger<CronusHeartbeat> logger)
    {
        this.publisher = publisher;
        this.boundedContext = boundedContext.CurrentValue;
        tenants = tenantsOptions.CurrentValue;
        options = heartbeatOptions.CurrentValue;
        this.logger = logger;

        heartbeatOptions.OnChange(OnHeartbeatOptionsChanged);
        tenantsOptions.OnChange(OnTenantsOptionsChanged);
    }

    public async Task StartBeatingAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            try
            {
                Dictionary<string, string> heartbeatHeaders = new Dictionary<string, string>() { { MessageHeader.TTL, TTL } };
                var signal = new HeartbeatSignal(boundedContext.Name, tenants.Tenants.ToList());
                publisher.Publish(signal, heartbeatHeaders);

                await Task.Delay(TimeSpan.FromSeconds(options.IntervalInSeconds), stoppingToken);
            }
            catch (Exception ex) when (ex is TaskCanceledException or ObjectDisposedException)
            {
                // Someone has cancled the task during the delay. In this case we just return without any error.
            }
            catch (Exception ex) when (True(() => logger.LogWarning(ex, "Failed to send heartbeat."))) { }
        }

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Heartbeat has been stopped.");
    }

    private void OnHeartbeatOptionsChanged(HeartbeatOptions newOptions)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Heartbeat options re-loaded with {@options}", newOptions);

        options = newOptions;
    }

    private void OnTenantsOptionsChanged(TenantsOptions newOptions)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Cronus tenants options re-loaded with {@options}", newOptions);

        tenants = newOptions;
    }
}
