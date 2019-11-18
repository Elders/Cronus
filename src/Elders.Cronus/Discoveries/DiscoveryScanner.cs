using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Logging;

namespace Elders.Cronus.Discoveries
{
    public sealed class DiscoveryScanner
    {
        private readonly static ILog log = LogProvider.GetLogger(typeof(DiscoveryScanner));

        public IEnumerable<IDiscoveryResult<object>> Scan(DiscoveryContext context)
        {
            var discoveries = context.Assemblies
                .SelectMany(asm => asm
                    .GetLoadableTypes()
                    .Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery<object>).IsAssignableFrom(type)))
                .Select(dt => (IDiscovery<object>)FastActivator.CreateInstance(dt));

            foreach (var discovery in discoveries)
            {
                log.Info($"Discovered {discovery.Name}");

                yield return discovery.Discover(context);
            }
        }
    }
}
