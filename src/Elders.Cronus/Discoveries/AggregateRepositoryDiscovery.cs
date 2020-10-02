using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class AggregateRepositoryDiscovery : DiscoveryBase<IAggregateRepository>
    {
        protected override DiscoveryResult<IAggregateRepository> DiscoverFromAssemblies(DiscoveryContext context)
        {
            IEnumerable<DiscoveredModel> models =
               DiscoverEventStreamIntegrityPolicy<EventStreamIntegrityPolicy>(context)
               .Concat(DiscoverAggregateRepository(context));

            return new DiscoveryResult<IAggregateRepository>(models);
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverAggregateRepository(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(AggregateRepository), typeof(AggregateRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(CronusAggregateRepository), provider => new CronusAggregateRepository(provider.GetRequiredService<AggregateRepository>(), provider.GetRequiredService<IPublisher<IEvent>>(), provider.GetRequiredService<IPublisher<IPublicEvent>>(), provider.GetRequiredService<CronusContext>()), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IAggregateRepository), provider => provider.GetRequiredService<CronusAggregateRepository>(), ServiceLifetime.Transient);
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverEventStreamIntegrityPolicy<TIntegrityPolicy>(DiscoveryContext context) where TIntegrityPolicy : IIntegrityPolicy<EventStream>
        {
            return DiscoverModel<IIntegrityPolicy<EventStream>, TIntegrityPolicy>(ServiceLifetime.Transient);
        }
    }
}
