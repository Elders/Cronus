using System;
using System.Collections.Generic;

namespace NMSD.Cronus.Pipelining
{
    public interface IEndpointNameConvention
    {
        IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes);
    }
}