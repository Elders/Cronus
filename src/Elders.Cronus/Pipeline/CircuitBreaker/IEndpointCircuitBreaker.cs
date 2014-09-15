using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline;

namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public interface IEndpointCircuitBreaker
    {
        ICircuitBreakerSuccessStrategy SuccessStrategy { get; set; }

        ICircuitBreakerRetryStrategy RetryStrategy { get; set; }

        ICircuitBreakerErrorStrategy ErrorStrategy { get; set; }

        void PostConsume(ISafeBatchResult<TransportMessage> mesages);
    }
}
