using System;
using Elders.Cronus.Serializer;

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

        protected abstract void Handle(DiscoveryResult<ISerializer> discoveredModel);

        protected abstract void Handle(DiscoveryResult<ITransport> discoveredModel);
    }
}
