using System;
using System.Collections.Generic;
using System.IO;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Transports;
using NMSD.Protoreg;

namespace NMSD.Cronus.Core.Eventing
{
    public class EventPublisher : IPublisher<IEvent>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventPublisher));

        private readonly IEventHandlersPipelineConvention convetion;

        private readonly IPipelineFactory pipelineFactory;

        Dictionary<Type, IPipeline> pipelines = new Dictionary<Type, IPipeline>();

        private readonly ProtoregSerializer serializer;

        public EventPublisher(IEventHandlersPipelineConvention convetion, IPipelineFactory pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.convetion = convetion;
            this.serializer = serializer;
        }

        public bool Publish(IEvent @event)
        {
            var endpointMessage = ToEndpointMessage(@event);
            var eventType = @event.GetType();
            endpointMessage.Headers.Add(MessageInfo.GetMessageId(eventType), String.Empty);
            BuildPipeline(eventType);
            pipelines[eventType].Push(endpointMessage);
            log.Info("PUBLISHED EVENT => " + @event.ToString());
            return true;
        }

        private EndpointMessage ToEndpointMessage(IEvent evnt)
        {
            byte[] body;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, evnt);
                body = stream.ToArray();
            }
            return new EndpointMessage(body);
        }

        private void BuildPipeline(Type eventType)
        {
            if (!pipelines.ContainsKey(eventType))
            {
                var pipelineName = convetion.GetPipelineName(eventType);
                var pipeline = pipelineFactory.GetPipeline(pipelineName);
                pipelines[eventType] = pipeline;
            }
        }
    }

}