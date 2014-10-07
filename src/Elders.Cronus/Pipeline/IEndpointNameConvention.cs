using System;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointNameConvention
    {
        EndpointDefinition GetEndpointDefinition(params Type[] handlerTypes);
    }

    public interface IAppServiceEndpointNameConvention : IEndpointNameConvention { }
    public interface IPortEndpointNameConvention : IEndpointNameConvention { }
    public interface IProjectionEndpointNameConvention : IEndpointNameConvention { }
}