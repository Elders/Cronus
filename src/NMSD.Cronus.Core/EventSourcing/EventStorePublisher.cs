using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NMSD.Cronus.Core.DomainModelling;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Transports;
using NMSD.Cronus.RabbitMQ;
using NMSD.Protoreg;

namespace NMSD.Cronus.Core.EventSourcing
{
    public class EventStorePublisher : IPublisher<DomainMessageCommit>
    {
        private readonly IEventStorePipelineConvention pipelineConvention;
        private readonly IPipelineFactory pipelineFactory;

        public EventStorePublisher(IEventStorePipelineConvention pipelineConvention, IPipelineFactory pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineConvention = pipelineConvention;
            this.pipelineFactory = pipelineFactory;
            this.serializer = serializer;

        }

        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventStorePublisher));

        Dictionary<Type, IPipeline> pipelines = new Dictionary<Type, IPipeline>();

        private readonly ProtoregSerializer serializer;

        public bool Publish(DomainMessageCommit commit)
        {
            var firstEventInCommitType = commit.Events.First().GetType();
            var endpointMessage = ToEndpointMessage(commit);
            var commitBoundedContext = firstEventInCommitType.GetAssemblyAttribute<BoundedContextAttribute>().BoundedContextName;
            endpointMessage.Headers.Add(commitBoundedContext, String.Empty);
            BuildPipeline(firstEventInCommitType);
            pipelines[firstEventInCommitType].Push(endpointMessage);
            log.Info("PUBLISHED COMMIT => " + commit.ToString());
            return true;
        }

        private EndpointMessage ToEndpointMessage(DomainMessageCommit cmd)
        {
            byte[] body;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, cmd);
                body = stream.ToArray();
            }
            return new EndpointMessage(body);
        }

        private void BuildPipeline(Type eventType)
        {
            if (!pipelines.ContainsKey(eventType))
            {
                var pipelineName = pipelineConvention.GetPipelineName(eventType.Assembly);
                var pipeline = pipelineFactory.GetPipeline(pipelineName);
                pipelines[eventType] = pipeline;
            }
        }
    }
}