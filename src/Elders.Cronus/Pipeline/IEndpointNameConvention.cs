using System;
using System.Collections.Generic;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointNameConvention
    {
        EndpointDefinition GetEndpointDefinition(params Type[] handlerTypes);
    }
}