using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Logging;

namespace Elders.Cronus.Discoveries
{
    public class DiscoveryScanner : DiscoveryBasedOnExecutingDirAssemblies
    {
        private readonly static ILog log = LogProvider.GetLogger(typeof(DiscoveryScanner));
        private readonly CronusServices cronusServices;

        public DiscoveryScanner(CronusServices cronusServices)
        {
            this.cronusServices = cronusServices;
        }

        protected override DiscoveryResult DiscoverFromAssemblies(DiscoveryContext context)
        {
            var discoveries = context.Assemblies
                .SelectMany(asm =>
                {
                    IEnumerable<Type> exportedTypes = asm.GetExportedTypes();
                    return exportedTypes.Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery).IsAssignableFrom(type) && type != typeof(DiscoveryScanner));
                })
                .Select(dt => (IDiscovery)FastActivator.CreateInstance(dt));

            foreach (IDiscovery discovery in discoveries)
            {
                log.Info($"Discovered {discovery.Name}");
                var discoveryResult = discovery.Discover();
                cronusServices.Handle(discoveryResult);
            }

            return new DiscoveryResult();
        }
    }
}
