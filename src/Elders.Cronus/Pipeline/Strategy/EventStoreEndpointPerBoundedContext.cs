using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline.Strategy
{
    public class EventStoreEndpointPerBoundedContext : IEndpointNameConvention
    {
        IPipelineNameConvention pipelineNameConvention;

        public EventStoreEndpointPerBoundedContext(IPipelineNameConvention pipelineNameConvention)
        {
            this.pipelineNameConvention = pipelineNameConvention;
        }

        public EndpointDefinition GetEndpointDefinition(params Type[] eventTypes)
        {
            var eventType = eventTypes.First();
            var boundedContext = eventType.GetBoundedContext();
            var handlerQueueName = String.Format("{0}.EventStore", boundedContext.BoundedContextNamespace);
            EndpointDefinition endpoint = new EndpointDefinition(pipelineNameConvention.GetPipelineName(eventType), handlerQueueName, new Dictionary<string, object> { { boundedContext.BoundedContextName, String.Empty } });
            return endpoint;
        }

    }
}