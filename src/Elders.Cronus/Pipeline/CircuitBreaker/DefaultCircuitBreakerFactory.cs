namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public class DefaultCircuitBreakerFactory : IEndpontCircuitBreakerFactrory
    {
        public IEndpointCircuitBreaker Create(Transport.IPipelineTransport transport, Serializer.ISerializer serializer, EndpointDefinition definitioin)
        {
            return new DefaultEndpointCircuitBreaker(transport, serializer, definitioin);
        }
    }
}
