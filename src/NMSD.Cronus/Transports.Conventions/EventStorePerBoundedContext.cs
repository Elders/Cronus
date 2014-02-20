using System;
using System.Collections.Generic;
using NMSD.Cronus.DomainModelling;
using System.Reflection;

namespace NMSD.Cronus.Transports.Conventions
{
    public class EventStorePerBoundedContext : IEventStoreEndpointConvention
    {
        IPipelineNameConvention pipelineNameConvention;

        public EventStorePerBoundedContext(IPipelineNameConvention pipelineNameConvention)
        {
            this.pipelineNameConvention = pipelineNameConvention;
        }

        public IEnumerable<EndpointDefinition> GetEndpointDefinitions(Type eventType)
        {
            Assembly assemblyContainingEvents = eventType.Assembly;
            var atr = assemblyContainingEvents.GetAssemblyAttribute<BoundedContextAttribute>();
            var handlerQueueName = String.Format("{0}.EventStore", atr.BoundedContextNamespace);
            var endpoint = new EndpointDefinition(handlerQueueName, new Dictionary<string, object> { { atr.BoundedContextName, String.Empty } }, pipelineNameConvention.GetPipelineName(eventType));
            yield return endpoint;
        }

    }
}