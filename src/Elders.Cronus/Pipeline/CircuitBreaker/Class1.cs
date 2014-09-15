using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public class DefaultCircuitBreakerFactory : IEndpontCircuitBreakerFactrory
    {
        public IEndpointCircuitBreaker Create(Pipeline.Transport.IPipelineTransport transport, Serializer.ISerializer serializer, Pipeline.EndpointDefinition definitioin)
        {
            return new DefaultEndpointCircuitBreaker(transport, serializer, definitioin);
        }
    }
}
