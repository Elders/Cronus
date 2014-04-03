using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline;
using Elders.Protoreg;

namespace Elders.Cronus.EventSourcing
{
    public class EventStorePublisher : Publisher<DomainMessageCommit>
    {
        private readonly IPipelineFactory<IPipeline> pipelineFactory;

        public EventStorePublisher(IPipelineFactory<IPipeline> pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.serializer = serializer;
        }

        private readonly ProtoregSerializer serializer;

        protected override bool PublishInternal(DomainMessageCommit message)
        {
            var firstEventInCommitType = message.Events.First().GetType();
            var commitBoundedContext = firstEventInCommitType.GetBoundedContext().BoundedContextName;
            var endpointMessage = message.AsEndpointMessage(serializer, routingHeaders: new Dictionary<string, object>() { { commitBoundedContext, String.Empty } });
            pipelineFactory
                .GetPipeline(firstEventInCommitType)
                .Push(endpointMessage);
            return true;
        }
    }
}