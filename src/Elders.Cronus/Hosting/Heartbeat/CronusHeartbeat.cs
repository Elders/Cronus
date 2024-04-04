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
    private readonly List<string> tenants;
    private readonly HeartbeatOptions options;
    private readonly ILogger<CronusHeartbeat> logger;
    private const string TTL = "5000";
    private static Dictionary<string, string> heartbeatHeaders = new Dictionary<string, string>() { { MessageHeader.TTL, TTL } };

    public CronusHeartbeat(IPublisher<ISignal> publisher, IOptionsMonitor<BoundedContext> boundedContext, IOptionsMonitor<HeartbeatOptions> HeartbeatOptions, IOptions<TenantsOptions> tenantsOptions, ILogger<CronusHeartbeat> logger)
    {
        this.publisher = publisher;
        this.boundedContext = boundedContext.CurrentValue;
        tenants = tenantsOptions.Value.Tenants.ToList();
        options = HeartbeatOptions.CurrentValue;
        this.logger = logger;
    }

    public async Task StartBeatingAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            try
            {
                var signal = new HeartbeatSignal(boundedContext.Name, tenants);
                publisher.Publish(signal, heartbeatHeaders);
                logger.Debug(() => "Heartbeat sent");
                await Task.Delay(TimeSpan.FromSeconds(options.IntervalInSeconds), stoppingToken);
            }
            catch (Exception ex) when (ex is TaskCanceledException or ObjectDisposedException)
            {
                // Someone has cancled the task during the delay. In this case we just return without any error.

            }
            catch (Exception ex)
            {
                // failed to send heartbeat 
                logger.WarnException(ex, () => "Failed to send heartbeat.");
            }
        }

        logger.LogInformation("Heartbeat has been stopped.");
    }
}
