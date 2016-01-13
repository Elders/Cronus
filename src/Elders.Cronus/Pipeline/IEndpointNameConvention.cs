namespace Elders.Cronus.Pipeline
{
    public interface IEndpointNameConvention
    {
        System.Collections.Generic.IEnumerable<EndpointDefinition> GetEndpointDefinition(IMessageProcessor messageProcessor);
    }

    public abstract class EndpointNameConvention : IEndpointNameConvention
    {
        public abstract System.Collections.Generic.IEnumerable<EndpointDefinition> GetEndpointDefinition(IMessageProcessor messageProcessor);
    }
}
