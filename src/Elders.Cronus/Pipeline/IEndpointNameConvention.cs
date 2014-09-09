using System;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointNameConvention
    {
        EndpointDefinition GetEndpointDefinition(params Type[] handlerTypes);
    }
}