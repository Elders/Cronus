using System;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.EventStore;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Middleware;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Pipeline;
using Elders.Cronus.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class CronusServicesProvider
    {
        private readonly IServiceCollection services;

        public CronusServicesProvider(IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            this.services = services;
        }

        public void HandleDiscoveredModel(IDiscoveryResult<object> discoveryResult)
        {
            if (discoveryResult is null) throw new ArgumentNullException(nameof(discoveryResult));

            dynamic dynamicModel = (dynamic)discoveryResult;
            Handle(dynamicModel);
        }

        void AddServices(IDiscoveryResult<object> discoveryResult)
        {
            foreach (var discoveredModel in discoveryResult.Models)
            {
                services.Add(discoveredModel);
            }
        }

        protected virtual void Handle(DiscoveryResult<ISerializer> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IConsumer<object>> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IPublisher<IMessage>> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IProjectionReader> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IProjectionWriter> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IAggregateRootAtomicAction> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IAggregateRootApplicationService> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IProjection> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IPort> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<ISaga> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IGateway> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IMiddleware> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IHandlerFactory> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IEventStore> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<IAggregateRepository> discoveryResult) => AddServices(discoveryResult);

        protected virtual void Handle(DiscoveryResult<ITenantList> discoveryResult) => AddServices(discoveryResult);
    }
}
