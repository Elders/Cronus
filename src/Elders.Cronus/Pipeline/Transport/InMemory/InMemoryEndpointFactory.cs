using System;

namespace Elders.Cronus.Pipeline.Transport.InMemory
{
    public class InMemoryEndpointFactory : IEndpointFactory
    {
        private readonly IEndpointNameConvention endpointNameConvention;
        private readonly InMemoryPipelineTransport transport;

        public InMemoryEndpointFactory(InMemoryPipelineTransport transport, IEndpointNameConvention endpointNameConvention)
        {
            this.transport = transport;
            this.endpointNameConvention = endpointNameConvention;
        }

        public IEndpoint CreateEndpoint(EndpointDefinition definition)
        {
            return transport.GetOrAddEndpoint(definition);
        }

        public EndpointDefinition GetEndpointDefinition(params Type[] handlerTypes)
        {
            return endpointNameConvention.GetEndpointDefinition(handlerTypes);
        }


        public IEndpoint CreateTopicEndpoint(EndpointDefinition definition)
        {
            return transport.GetOrAddEndpoint(definition);
        }
    }
}