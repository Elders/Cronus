using System;
using System.Collections.Generic;

namespace NMSD.Cronus.Transports
{
    public interface IEndpointFactory
    {
        IEndpoint CreateEndpoint(EndpointDefinition definition);

        IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes);
    }
}