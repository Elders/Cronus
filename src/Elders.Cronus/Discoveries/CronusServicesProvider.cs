using System;
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
    }
}
