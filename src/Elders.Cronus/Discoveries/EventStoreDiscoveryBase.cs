using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.EventStore.InMemory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Discoveries
{
    public abstract class EventStoreDiscoveryBase : DiscoveryBase<IEventStore>
    {
        protected override DiscoveryResult<IEventStore> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IEventStore>(GetModels(context));
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            var loadedTypes = context.Assemblies.Find<IEventStoreIndex>();
            yield return new DiscoveredModel(typeof(EventToAggregateRootId), typeof(EventToAggregateRootId), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(TypeContainer<IEventStoreIndex>), new TypeContainer<IEventStoreIndex>(loadedTypes));
        }
    }

    public class InMemoryEventStoreDiscovery : EventStoreDiscoveryBase
    {
        protected override DiscoveryResult<IEventStore> DiscoverFromAssemblies(DiscoveryContext context)
        {
            var baseResult = base.DiscoverFromAssemblies(context);
            var inMemoryModels = GetModels(context);
            return new DiscoveryResult<IEventStore>(baseResult.Models.Concat(inMemoryModels));
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(IEventStore), typeof(InMemoryEventStore), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(InMemoryEventStoreStorage), typeof(InMemoryEventStoreStorage), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(IIndexStore), typeof(InMemoryIndexStore), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(IEventStorePlayer), typeof(InMemoryEventStorePlayer), ServiceLifetime.Singleton);
        }
    }
}
