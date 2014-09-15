using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.Pipeline;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public interface IEndpontCircuitBreakerFactrory
    {
        IEndpointCircuitBreaker Create(IPipelineTransport transport, ISerializer serializer, EndpointDefinition definitioin);
    }
}
