using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            if ("true".Equals(context.Configuration["Cronus:PublishAggregateCommits"], System.StringComparison.OrdinalIgnoreCase))
            {
                yield return new DiscoveredModel(typeof(AggregateRepositoryAndEventPublisher), typeof(AggregateRepositoryAndEventPublisher), ServiceLifetime.Transient);

                yield return new DiscoveredModel(typeof(AggregateCommitPublisherRepository), provider =>
                    new AggregateCommitPublisherRepository(
                        provider.GetRequiredService<AggregateRepository>(),
                        provider.GetRequiredService<IPublisher<AggregateCommit>>(),
                        provider.GetRequiredService<ICronusContextAccessor>(),
                        provider.GetService<ILogger<AggregateCommitPublisherRepository>>()),
                    ServiceLifetime.Transient);

                yield return new DiscoveredModel(typeof(CronusAggregateRepository), provider =>
                    new CronusAggregateRepository(provider.GetRequiredService<AggregateRepositoryAndEventPublisher>()), ServiceLifetime.Transient);
            }
            else
            {
                yield return new DiscoveredModel(typeof(AggregateRepositoryAndEventPublisher), typeof(AggregateRepositoryAndEventPublisher), ServiceLifetime.Transient);

                yield return new DiscoveredModel(typeof(CronusAggregateRepository), provider =>
                    new CronusAggregateRepository(provider.GetRequiredService<AggregateRepositoryAndEventPublisher>()), ServiceLifetime.Transient);
            }

            yield return new DiscoveredModel(typeof(LoggingAggregateRepository), provider =>
                new LoggingAggregateRepository(
                    provider.GetRequiredService<CronusAggregateRepository>(),
                    provider.GetService<ILogger<LoggingAggregateRepository>>()),
                ServiceLifetime.Transient);

            yield return new DiscoveredModel(typeof(IAggregateRepository),
                provider => provider.GetRequiredService<SingletonPerTenant<LoggingAggregateRepository>>().Get(), ServiceLifetime.Transient);
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverEventStreamIntegrityPolicy<TIntegrityPolicy>(DiscoveryContext context) where TIntegrityPolicy : IIntegrityPolicy<EventStream>
        {
            return DiscoverModel<IIntegrityPolicy<EventStream>, TIntegrityPolicy>(ServiceLifetime.Transient);
        }
    }
}
