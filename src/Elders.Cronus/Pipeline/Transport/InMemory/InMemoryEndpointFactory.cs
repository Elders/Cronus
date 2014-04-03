using System;
using System.Collections.Generic;

namespace Elders.Cronus.Pipeline.Transport.InMemory
{
    public class InMemoryEndpointFactory : IEndpointFactory
    {
        private readonly IEndpointNameConvention endpointNameConvention;
        private readonly InMemoryPipelineFactory pipelineFactory;

        public InMemoryEndpointFactory(InMemoryPipelineFactory pipelineFactory, IEndpointNameConvention endpointNameConvention)
        {
            this.endpointNameConvention = endpointNameConvention;
            this.pipelineFactory = pipelineFactory;
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