using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
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
               .Concat(DiscoverAggregateRepository(context))
               .Concat(DiscoverAggregateInterceptors(context));

            return new DiscoveryResult<IAggregateRepository>(models);
        }

        private IEnumerable<DiscoveredModel> DiscoverAggregateInterceptors(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(CronusAggregateCommitInterceptor), typeof(CronusAggregateCommitInterceptor), ServiceLifetime.Singleton);

            var interceptors = context.Assemblies.Find<IAggregateCommitInterceptor>();
            foreach (var interceptorImpl in interceptors)
            {
                if (typeof(AggregateCommitPublisher).IsAssignableFrom(interceptorImpl))
                    continue;

                if (typeof(CronusAggregateCommitInterceptor).IsAssignableFrom(interceptorImpl))
                    continue;

                yield return new DiscoveredModel(typeof(IAggregateCommitInterceptor), interceptorImpl, ServiceLifetime.Singleton) { CanAddMultiple = true };
                yield return new DiscoveredModel(interceptorImpl, interceptorImpl, ServiceLifetime.Singleton);
            }

            bool shouldPublishCommits = "true".Equals(context.Configuration["Cronus:PublishAggregateCommits"], StringComparison.OrdinalIgnoreCase);
            if (shouldPublishCommits)
            {
                yield return new DiscoveredModel(typeof(IAggregateCommitInterceptor), typeof(AggregateCommitPublisher), ServiceLifetime.Singleton) { CanAddMultiple = true };
                yield return new DiscoveredModel(typeof(AggregateCommitPublisher), typeof(AggregateCommitPublisher), ServiceLifetime.Singleton);
            }
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverAggregateRepository(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(AggregateRepository), typeof(AggregateRepository), ServiceLifetime.Transient);

            yield return new DiscoveredModel(typeof(AggregateRepositoryAndEventPublisher), typeof(AggregateRepositoryAndEventPublisher), ServiceLifetime.Transient);

            yield return new DiscoveredModel(typeof(CronusAggregateRepository), provider =>
                new CronusAggregateRepository(provider.GetRequiredService<AggregateRepositoryAndEventPublisher>()), ServiceLifetime.Transient);

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
