using System;
using System.Collections.Generic;

namespace NMSD.Cronus.Pipelining
{
    public interface IEndpointFactory
    {
        IEndpoint CreateEndpoint(EndpointDefinition definition);

        IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes);
    }
}