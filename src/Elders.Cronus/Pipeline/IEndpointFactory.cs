using System;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointFactory
    {
        IEndpoint CreateEndpoint(EndpointDefinition definition);
        IEndpoint CreateTopicEndpoint(EndpointDefinition definition);

        EndpointDefinition GetEndpointDefinition(params Type[] handlerTypes);
    }
}