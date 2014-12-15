using System.Linq;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public class DefaultEndpointCircuitBreaker : IEndpointCircuitBreaker
    {
        public DefaultEndpointCircuitBreaker(IPipelineTransport transport, ISerializer serializer, EndpointDefinition definitioin)
        {
            this.ErrorStrategy = new EndpointPostConsumeStrategy.ErrorEndpointPerEndpoint(transport, serializer, definitioin);
            this.RetryStrategy = new EndpointPostConsumeStrategy.RetryEndpointPerEndpoint(transport, serializer, definitioin);
            this.SuccessStrategy = new EndpointPostConsumeStrategy.NoSuccessStrategy();
        }
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DefaultEndpointCircuitBreaker));

        public ICircuitBreakerErrorStrategy ErrorStrategy { get; set; }

        public ICircuitBreakerRetryStrategy RetryStrategy { get; set; }

        public ICircuitBreakerSuccessStrategy SuccessStrategy { get; set; }

        public void PostConsume(IFeedResult mesages)
        {
            foreach (var msg in mesages.FailedMessages)
            {
                if (msg.Age > 5)
                {
                    log.Error(msg.Payload, msg.Errors.First().Error);
                    ErrorStrategy.Handle(msg);
                }
                else
                {
                    msg.Age = msg.Age++;
                    RetryStrategy.Handle(msg);
                }
            }
        }
    }
}