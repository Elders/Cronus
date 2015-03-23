using System;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointNameConvention
    {
        System.Collections.Generic.IEnumerable<EndpointDefinition> GetEndpointDefinition(IMessageProcessor messageProcessor);
    }

    public abstract class EndpointNameConvention : IEndpointNameConvention
    {
        protected abstract string GetPortEndpointName(Type handlerType);
        protected abstract string GetProjectionEndpointName(Type handlerType);
        protected abstract string GetAppServiceEndpointName(Type handlerType);

        public abstract System.Collections.Generic.IEnumerable<EndpointDefinition> GetEndpointDefinition(IMessageProcessor messageProcessor);
    }
}
