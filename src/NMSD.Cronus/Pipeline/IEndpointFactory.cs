using System;
using System.Collections.Generic;

namespace NMSD.Cronus.Pipeline
{
    public interface IEndpointFactory
    {
        IEndpoint CreateEndpoint(EndpointDefinition definition);

        IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes);
    }
}