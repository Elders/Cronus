using System;

namespace Elders.Cronus.Pipeline.Transport.InMemory
{
    public class InMemoryEndpointFactory : IEndpointFactory
    {
        private readonly IEndpointNameConvention endpointNameConvention;

        public InMemoryEndpointFactory(IEndpointNameConvention endpointNameConvention)
        {
            this.endpointNameConvention = endpointNameConvention;
        }

        public IEndpoint CreateEndpoint(EndpointDefinition definition)
        {
            return InMemoryQueue.Current.GetOrAddEndpoint(definition);
        }

        public EndpointDefinition GetEndpointDefinition(params Type[] handlerTypes)
        {
            return endpointNameConvention.GetEndpointDefinition(handlerTypes);
        }


        public IEndpoint CreateTopicEndpoint(EndpointDefinition definition)
        {
            return InMemoryQueue.Current.GetOrAddEndpoint(definition);
        }
    }
}