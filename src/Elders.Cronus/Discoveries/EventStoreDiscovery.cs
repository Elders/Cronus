using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.EventStore.InMemory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Discoveries
{
    public class EventStoreDiscovery : DiscoveryBase<IEventStore>
    {
        protected override DiscoveryResult<IEventStore> DiscoverFromAssemblies(DiscoveryContext context)
        {
            IEnumerable<DiscoveredModel> models =
                DiscoverEventStore<InMemoryEventStore>(context)
                .Concat(DiscoverEventStorePlayer<InMemoryEventStorePlayer>(context))
                .Concat(DiscoverIndexStore<InMemoryIndexStore>(context))
                .Concat(DiscoverIndices(context));

            return new DiscoveryResult<IEventStore>(models);
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverEventStore<TEventStore>(DiscoveryContext context) where TEventStore : IEventStore
        {
            return DiscoverModel<IEventStore, TEventStore>(ServiceLifetime.Singleton)
                .Concat(new[] { new DiscoveredModel(typeof(InMemoryEventStoreStorage), typeof(InMemoryEventStoreStorage), ServiceLifetime.Singleton) });
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverIndexStore<TIndexStore>(DiscoveryContext context) where TIndexStore : IIndexStore
        {
            return DiscoverModel<IIndexStore, TIndexStore>(ServiceLifetime.Singleton);
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverEventStorePlayer<TEventStorePlayer>(DiscoveryContext context) where TEventStorePlayer : IEventStorePlayer
        {
            return DiscoverModel<TEventStorePlayer, TEventStorePlayer>(ServiceLifetime.Singleton);
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverIndices(DiscoveryContext context)
        {
            var loadedTypes = context.Assemblies.Find<IEventStoreIndex>();
            yield return new DiscoveredModel(typeof(TypeContainer<IEventStoreIndex>), new TypeContainer<IEventStoreIndex>(loadedTypes));

            foreach (var indexDef in loadedTypes)
            {
                yield return new DiscoveredModel(indexDef, indexDef, ServiceLifetime.Scoped);
            }
        }
    }
}
