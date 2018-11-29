using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class AggregateRepositoryDiscovery : DiscoveryBasedOnExecutingDirAssemblies<IAggregateRepository>
    {
        protected override DiscoveryResult<IAggregateRepository> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IAggregateRepository>(GetModels());
        }

        IEnumerable<DiscoveredModel> GetModels()
        {
            yield return new DiscoveredModel(typeof(IIntegrityPolicy<EventStream>), typeof(EventStreamIntegrityPolicy), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(AggregateRepository), typeof(AggregateRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IAggregateRepository), provider => new CronusAggregateRepository(provider.GetRequiredService<AggregateRepository>(), provider.GetRequiredService<IPublisher<IEvent>>()), ServiceLifetime.Transient);
        }
    }
}
