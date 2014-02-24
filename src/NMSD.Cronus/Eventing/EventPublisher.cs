using System;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports;
using NMSD.Protoreg;

namespace NMSD.Cronus.Eventing
{
    public class EventPublisher : Publisher<IEvent>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EventPublisher));

        private readonly IPipelineFactory<IPipeline> pipelineFactory;

        private readonly ProtoregSerializer serializer;

        public EventPublisher(IPipelineFactory<IPipeline> pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.serializer = serializer;
        }

        protected override bool PublishInternal(IEvent message)
        {
            var endpointMessage = message.AsEndpointMessage(serializer);
            var eventType = message.GetType();
            endpointMessage.Headers.Add(MessageInfo.GetContractId(eventType), String.Empty);
            pipelineFactory
                .GetPipeline(eventType)
                .Push(endpointMessage);
            return true;
        }
    }
}