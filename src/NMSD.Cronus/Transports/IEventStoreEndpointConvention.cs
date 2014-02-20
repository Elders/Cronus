using System;
using System.Collections.Generic;

namespace NMSD.Cronus.Transports.Conventions
{
    public interface IEventStoreEndpointConvention
    {
        IEnumerable<EndpointDefinition> GetEndpointDefinitions(Type eventType);
    }
}