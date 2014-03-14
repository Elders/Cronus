using System;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Transports;
using NMSD.Protoreg;

namespace NMSD.Cronus.Pipelining
{
    public class PipelinePublisher<T> : Publisher<T> where T : IMessage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PipelinePublisher<T>));

        private readonly IPipelineFactory<IPipeline> pipelineFactory;

        private readonly ProtoregSerializer serializer;

        public PipelinePublisher(IPipelineFactory<IPipeline> pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.serializer = serializer;
        }

        protected override bool PublishInternal(T message)
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