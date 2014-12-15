using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public class NoCircuitBreakerFactory : IEndpontCircuitBreakerFactrory
    {
        public IEndpointCircuitBreaker Create(Transport.IPipelineTransport transport, Serializer.ISerializer serializer, EndpointDefinition definitioin)
        {
            return new NoEndpointPostConsume();
        }
    }
    public class NoEndpointPostConsume : IEndpointCircuitBreaker
    {
        public NoEndpointPostConsume()
        {
            this.ErrorStrategy = new EndpointPostConsumeStrategy.NoErrorStrategy();
            this.RetryStrategy = new EndpointPostConsumeStrategy.NoRetryStrategy();
            this.SuccessStrategy = new EndpointPostConsumeStrategy.NoSuccessStrategy();
        }

        public ICircuitBreakerErrorStrategy ErrorStrategy { get; set; }

        public ICircuitBreakerRetryStrategy RetryStrategy { get; set; }

        public ICircuitBreakerSuccessStrategy SuccessStrategy { get; set; }

        public void PostConsume(IFeedResult mesages)
        {

        }
    }
}
