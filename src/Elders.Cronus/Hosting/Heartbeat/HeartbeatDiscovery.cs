using Elders.Cronus.Discoveries;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Elders.Cronus.Hosting.Heartbeat
{
    public class HeartbeatDiscovery : DiscoveryBase<IHeartbeat>
    {
        protected override DiscoveryResult<IHeartbeat> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IHeartbeat>(GetModels(context), AddServices);
        }
        private IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(IHeartbeat), typeof(CronusHeartbeat), ServiceLifetime.Singleton);
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddHostedService<CronusHeartbeatService>();
        }
    }
}
