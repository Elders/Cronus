using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointFactory
    {
        IEndpoint CreateEndpoint(EndpointDefinition definition);
        IEndpoint CreateTopicEndpoint(EndpointDefinition definition);
        IEnumerable<EndpointDefinition> GetEndpointDefinition(SubscriptionMiddleware subscriptionMiddleware);
    }
}
