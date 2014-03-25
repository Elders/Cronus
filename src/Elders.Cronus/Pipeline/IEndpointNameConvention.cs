using System;
using System.Collections.Generic;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointNameConvention
    {
        IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes);
    }
}