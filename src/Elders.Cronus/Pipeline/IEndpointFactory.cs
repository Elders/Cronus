using Elders.Cronus.Netflix;
using System.Collections.Generic;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointFactory
    {
        IEndpoint CreateEndpoint(EndpointDefinition definition);
        IEndpoint CreateTopicEndpoint(EndpointDefinition definition);
        IEnumerable<EndpointDefinition> GetEndpointDefinition(SubscriptionMiddleware subscriptionMiddleware);
    }
}
