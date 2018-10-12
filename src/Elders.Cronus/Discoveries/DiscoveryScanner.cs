using System;
using System.Linq;
using Elders.Cronus.Logging;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Discoveries
{
    public class DiscoveryScanner : DiscoveryBasedOnExecutingDirAssemblies<DiscoveryScanner>
    {
        private readonly static ILog log = LogProvider.GetLogger(typeof(DiscoveryScanner));

        private readonly CronusServicesProvider cronusServicesProvider;

        public DiscoveryScanner(CronusServicesProvider cronusServicesProvider, IConfiguration configuration)
        {
            if (cronusServicesProvider is null) throw new ArgumentNullException(nameof(cronusServicesProvider));
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));

            this.cronusServicesProvider = cronusServicesProvider;
            Configuration = configuration;
        }

        protected override DiscoveryResult<DiscoveryScanner> DiscoverFromAssemblies(DiscoveryContext context)
        {
            var discoveries = context.Assemblies
                .SelectMany(asm => asm
                    .GetLoadableTypes()
                    .Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery<object>).IsAssignableFrom(type) && type != typeof(DiscoveryScanner)))
                .Select(dt => (IDiscovery<object>)FastActivator.CreateInstance(dt));

            foreach (var discovery in discoveries)
            {
                log.Info($"Discovered {discovery.Name}");
                discovery.AssignPropertySafely<IHaveConfiguration>(x => x.Configuration = context.Configuration);
                var discoveryResult = discovery.Discover();
                cronusServicesProvider.HandleDiscoveredModel(discoveryResult);
            }

            return new DiscoveryResult<DiscoveryScanner>();
        }
    }
}
