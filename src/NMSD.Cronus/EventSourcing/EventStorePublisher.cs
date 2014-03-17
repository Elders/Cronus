using System;
using System.Linq;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipelining;
using NMSD.Protoreg;

namespace NMSD.Cronus.EventSourcing
{
    public class EventStorePublisher : Publisher<DomainMessageCommit>
    {
        private readonly IPipelineFactory<IPipeline> pipelineFactory;

        public EventStorePublisher(IPipelineFactory<IPipeline> pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.serializer = serializer;
        }

        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventStorePublisher));

        private readonly ProtoregSerializer serializer;

        protected override bool PublishInternal(DomainMessageCommit message)
        {
            var firstEventInCommitType = message.Events.First().GetType();
            var endpointMessage = message.AsEndpointMessage(serializer);
            var commitBoundedContext = firstEventInCommitType.GetBoundedContext().BoundedContextName;
            endpointMessage.Headers.Add(commitBoundedContext, String.Empty);
            pipelineFactory
                .GetPipeline(firstEventInCommitType)
                .Push(endpointMessage);
            return true;
        }
    }
}