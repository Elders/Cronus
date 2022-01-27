using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Hosting.Heartbeat
{
    public class CronusHeartbeat : IHeartbeat
    {
        private readonly IPublisher<ISystemSignal> publisher;
        private readonly List<string> tenants;
        private readonly ILogger<CronusHeartbeat> logger;

        public string Name { get; set; } = "cronus";

        public CronusHeartbeat(IPublisher<ISystemSignal> publisher, IOptions<TenantsOptions> tenantsOptions, ILogger<CronusHeartbeat> logger)
        {
            this.publisher = publisher;
            tenants = tenantsOptions.Value.Tenants.ToList();
            this.logger = logger;
        }

        public async Task StartBeating(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    var signal = new HeartbeatSignal("cronus", tenants);
                    publisher.Publish(signal);

                    logger.Debug(() => "Heartbeat");

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // failed to send heartbeat
                    logger.WarnException(ex, () => "Failed to send heartbeat.");
                }
            }
        }
    }
}
