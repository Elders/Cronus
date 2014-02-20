using System;
using System.Linq;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports;
using NMSD.Protoreg;

namespace NMSD.Cronus.EventSourcing
{
    public class EventStorePublisher : IPublisher<DomainMessageCommit>
    {
        private readonly IPipelineFactory pipelineFactory;

        public EventStorePublisher(IPipelineFactory pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.serializer = serializer;
        }

        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventStorePublisher));

        private readonly ProtoregSerializer serializer;

        public bool Publish(DomainMessageCommit commit)
        {
            var firstEventInCommitType = commit.Events.First().GetType();
            var endpointMessage = commit.AsEndpointMessage(serializer);
            var commitBoundedContext = firstEventInCommitType.GetAssemblyAttribute<BoundedContextAttribute>().BoundedContextName;
            endpointMessage.Headers.Add(commitBoundedContext, String.Empty);
            pipelineFactory.GetPipeline(firstEventInCommitType).Push(endpointMessage);
            if (log.IsInfoEnabled)
                log.Info("PUBLISHED COMMIT => " + commit.ToString());
            return true;
        }
    }
}