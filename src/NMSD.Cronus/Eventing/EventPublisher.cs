using System;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports;
using NMSD.Protoreg;

namespace NMSD.Cronus.Eventing
{
    public class EventPublisher : IPublisher<IEvent>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventPublisher));

        private readonly IPipelineFactory pipelineFactory;

        private readonly ProtoregSerializer serializer;

        public EventPublisher(IPipelineFactory pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.serializer = serializer;
        }

        public bool Publish(IEvent @event)
        {
            var endpointMessage = @event.AsEndpointMessage(serializer);
            var eventType = @event.GetType();
            endpointMessage.Headers.Add(MessageInfo.GetMessageId(eventType), String.Empty);
            pipelineFactory.GetPipeline(eventType).Push(endpointMessage);
            log.Info("PUBLISHED EVENT => " + @event.ToString());
            return true;
        }
    }
}