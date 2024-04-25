using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Discoveries;

public class EventStoreDiscovery : DiscoveryBase<IEventStore>
{
    protected override DiscoveryResult<IEventStore> DiscoverFromAssemblies(DiscoveryContext context)
    {
        var cronusOptions = new CronusHostOptions();
        context.Configuration.GetSection("Cronus").Bind(cronusOptions);

        IEnumerable<DiscoveredModel> models = Enumerable.Empty<DiscoveredModel>();

        if (cronusOptions.ApplicationServicesEnabled)
        {
            models = DiscoverIndices(context)
              .Concat(new[] {
                new DiscoveredModel(typeof(IEventStoreFactory), typeof(EventStoreFactory), ServiceLifetime.Transient),
                new DiscoveredModel(typeof(EventStoreFactory), typeof(EventStoreFactory), ServiceLifetime.Transient),
                new DiscoveredModel(typeof(EventLookupInByteArray), typeof(EventLookupInByteArray), ServiceLifetime.Singleton)
              });
        }
        else
        {
            models = DiscoverIndices(context)
              .Concat(new[] {
                new DiscoveredModel(typeof(IEventStoreFactory), typeof(MIssingEventStoreFactory), ServiceLifetime.Singleton),
                new DiscoveredModel(typeof(EventLookupInByteArray), typeof(EventLookupInByteArray), ServiceLifetime.Singleton)
              });
        }

        return new DiscoveryResult<IEventStore>(models);
    }

    protected virtual IEnumerable<DiscoveredModel> DiscoverIndices(DiscoveryContext context)
    {
        var appIndices = context.Assemblies.Find<IEventStoreIndex>();

        yield return new DiscoveredModel(typeof(TypeContainer<IEventStoreIndex>), new TypeContainer<IEventStoreIndex>(appIndices));

        foreach (var indexDef in appIndices)
        {
            yield return new DiscoveredModel(indexDef, indexDef, ServiceLifetime.Scoped);
        }

        var systemIndices = context.Assemblies.Find<ICronusEventStoreIndex>();
        yield return new DiscoveredModel(typeof(TypeContainer<ICronusEventStoreIndex>), new TypeContainer<ICronusEventStoreIndex>(systemIndices));

        foreach (var indexDef in systemIndices)
        {
            yield return new DiscoveredModel(indexDef, indexDef, ServiceLifetime.Scoped);
        }
    }
}
