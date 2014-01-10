using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.DomainModelling;


namespace NMSD.Cronus.Core.Transports.Conventions
{
    public class EventStorePerBoundedContext : IEventStoreEndpointConvention
    {
        IEventStorePipelineConvention convention;

        public EventStorePerBoundedContext(IEventStorePipelineConvention convention)
        {
            this.convention = convention;
        }
        public IEnumerable<EndpointDefinition> GetEndpointDefinitions(System.Reflection.Assembly assemblyContainingEvents)
        {
            var atr = assemblyContainingEvents.GetAssemblyAttribute<BoundedContextAttribute>();
            var handlerQueueName = String.Format("{0}.EventStore", atr.BoundedContextNamespace);
            var endpoint = new EndpointDefinition(handlerQueueName, new Dictionary<string, object> { { atr.BoundedContextName, String.Empty } }, convention.GetPipelineName(assemblyContainingEvents));
            yield return endpoint;
        }
    }
}
