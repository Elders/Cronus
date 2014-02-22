using System;
using System.Collections.Generic;
using System.Linq;

namespace NMSD.Cronus.Transports.Conventions
{
    public class EventStorePerBoundedContext : IEndpointNameConvention
    {
        IPipelineNameConvention pipelineNameConvention;

        public EventStorePerBoundedContext(IPipelineNameConvention pipelineNameConvention)
        {
            this.pipelineNameConvention = pipelineNameConvention;
        }

        public IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] eventTypes)
        {
            var eventType = eventTypes.First();
            var boundedContext = eventType.GetBoundedContext();
            var handlerQueueName = String.Format("{0}.EventStore", boundedContext.BoundedContextNamespace);
            var endpoint = new EndpointDefinition(handlerQueueName, new Dictionary<string, object> { { boundedContext.BoundedContextName, String.Empty } }, pipelineNameConvention.GetPipelineName(eventType));
            yield return endpoint;
        }

    }
}