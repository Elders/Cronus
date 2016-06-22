using System.Linq;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;
using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessingMiddleware;
using System;

namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public class DefaultEndpointCircuitBreaker : IEndpointCircuitBreaker
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(DefaultEndpointCircuitBreaker));

        public DefaultEndpointCircuitBreaker(IPipelineTransport transport, ISerializer serializer, EndpointDefinition definitioin)
        {
            this.ErrorStrategy = new EndpointPostConsumeStrategy.ErrorEndpointPerEndpoint(transport, serializer, definitioin);
            this.RetryStrategy = new EndpointPostConsumeStrategy.RetryEndpointPerEndpoint(transport, serializer, definitioin);
            this.SuccessStrategy = new EndpointPostConsumeStrategy.NoSuccessStrategy();
            this.MaximumMessageAge = 5;
        }

        public ICircuitBreakerErrorStrategy ErrorStrategy { get; set; }

        public ICircuitBreakerRetryStrategy RetryStrategy { get; set; }

        public ICircuitBreakerSuccessStrategy SuccessStrategy { get; set; }

        public int MaximumMessageAge { get; private set; }

        public void PostConsume(IFeedResult mesages)
        {
            foreach (var msg in mesages.FailedMessages)
            {
                PostConsume(msg);
            }
        }

        public void PostConsume(CronusMessage msg)
        {
            if (msg.Errors.Any())
            {
                if (msg.Age > MaximumMessageAge)
                {
                    log.ErrorException(msg.Payload.ToString(), msg.Errors.First().Error);
                    ErrorStrategy.Handle(msg);
                }
                else
                {
                    log.WarnException(msg.Payload.ToString(), msg.Errors.First().Error);
                    msg.Age = msg.Age + 1;
                    RetryStrategy.Handle(msg);
                }
            }
        }
    }
}