using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline;
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

        public void PostConsume(ISafeBatchResult<TransportMessage> mesages)
        {
            foreach (var batch in mesages.FailedBatches)
            {
                foreach (var msg in batch.Items)
                {
                    if (msg.Age > 5)
                    {
                        log.Error(msg.Payload, batch.Error);
                        ErrorMessage errorMessage = new ErrorMessage(msg.Payload, batch.Error);
                        TransportMessage error = new TransportMessage(errorMessage);
                        ErrorStrategy.Handle(error);
                    }
                    else
                    {
                        RetryStrategy.Handle(msg);
                    }
                }
            }
        }
    }
}
