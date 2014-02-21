using System;
using System.Collections.Generic;

namespace NMSD.Cronus.Transports.Conventions
{
    public interface IEndpointNameConvention
    {
        IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes);
    }
}