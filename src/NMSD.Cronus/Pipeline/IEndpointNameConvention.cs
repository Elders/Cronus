using System;
using System.Collections.Generic;

namespace NMSD.Cronus.Pipeline
{
    public interface IEndpointNameConvention
    {
        IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes);
    }
}