using System;
using System.Collections.Generic;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections;

namespace Elders.Cronus.Discoveries
{
    public abstract class CronusServicesProvider
    {
        public void HandleDiscoveredModel(IDiscoveryResult<object> discoveryResult)
        {
            if (discoveryResult is null) throw new ArgumentNullException(nameof(discoveryResult));

            dynamic dynamicModel = (dynamic)discoveryResult;
            Handle(dynamicModel);
        }

        protected abstract void Handle(DiscoveryResult<ISerializer> discoveryResult);

        protected abstract void Handle(DiscoveryResult<ITransport> discoveryResult);

        protected abstract void Handle(DiscoveryResult<IProjectionLoader> discoveryResult);

        protected abstract void Handle(DiscoveryResult<IAggregateRootApplicationService> discoveryResult);

        protected abstract void Handle(DiscoveryResult<IHandlerFactory> discoveryResult);

        protected abstract void Handle(DiscoveryResult<IEventStore> discoveryResult);

        protected abstract void Handle(DiscoveryResult<IAggregateRepository> discoveryResult);

        protected abstract void Handle(DiscoveryResult<ITenantList> discoveryResult);
    }

    public class AggregateRepositoryDiscovery : DiscoveryBasedOnExecutingDirAssemblies<IAggregateRepository>
    {
        protected override DiscoveryResult<IAggregateRepository> DiscoverFromAssemblies(DiscoveryContext context)
        {
            var result = new DiscoveryResult<IAggregateRepository>();
            result.Models.AddRange(GetModels());

            return result;
        }

        IEnumerable<DiscoveredModel> GetModels()
        {
            yield return new DiscoveredModel(typeof(IIntegrityPolicy<EventStream>), typeof(EventStreamIntegrityPolicy));
            yield return new DiscoveredModel(typeof(IAggregateRepository), typeof(AggregateRepository));
        }
    }

    public class ApplicationServicesDiscovery : DiscoveryBasedOnExecutingDirAssemblies<IAggregateRootApplicationService>
    {
        protected override DiscoveryResult<IAggregateRootApplicationService> DiscoverFromAssemblies(DiscoveryContext context)
        {
            var result = new DiscoveryResult<IAggregateRootApplicationService>();
            result.Models.AddRange(GetModels());

            return result;
        }

        IEnumerable<DiscoveredModel> GetModels()
        {
            yield return new DiscoveredModel(typeof(ApplicationServiceMiddleware));
        }
    }
}
