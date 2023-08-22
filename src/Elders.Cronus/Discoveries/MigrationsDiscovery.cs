using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries;

public class MigrationsDiscovery : HandlersDiscovery<IMigrationHandler>
{
    protected override DiscoveryResult<IMigrationHandler> DiscoverFromAssemblies(DiscoveryContext context)
    {
        IEnumerable<DiscoveredModel> models =
            DiscoverHandlers(context)
            .Concat(DiscoverCustomLogic(context));

        return new DiscoveryResult<IMigrationHandler>(models);
    }

    protected virtual IEnumerable<DiscoveredModel> DiscoverCustomLogic(DiscoveryContext context)
    {
        yield return new DiscoveredModel(typeof(IMigrationCustomLogic), typeof(NoCustomLogic), ServiceLifetime.Transient)
        {
            CanOverrideDefaults = true
        };
    }
}
