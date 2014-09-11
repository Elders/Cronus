using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.EventSourcing
{
    public class EventStorePublisher : PipelinePublisher<DomainMessageCommit>
    {
        private readonly IPipelineTransport transport;

        public EventStorePublisher(IPipelineTransport pipelineFactory, ISerializer serializer)
            : base(pipelineFactory, serializer)
        {
            this.transport = pipelineFactory;
            this.serializer = serializer;
        }

        private readonly ISerializer serializer;

        protected override bool PublishInternal(DomainMessageCommit message)
        {
            var firstEventInCommitType = message.Events.First().GetType();
            var commitBoundedContext = firstEventInCommitType.GetBoundedContext().BoundedContextName;
            var endpointMessage = message.AsEndpointMessage(serializer, routingHeaders: new Dictionary<string, object>() { { commitBoundedContext, String.Empty } });
            transport.PipelineFactory
                .GetPipeline(firstEventInCommitType)
                .Push(endpointMessage);
            return true;
        }
    }
}