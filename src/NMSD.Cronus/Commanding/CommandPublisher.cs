using System;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports;
using NMSD.Protoreg;

namespace NMSD.Cronus.Commanding
{
    public class CommandPublisher : Publisher<ICommand>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CommandPublisher));

        private readonly IPipelineFactory<IPipeline> pipelineFactory;

        private readonly ProtoregSerializer serializer;

        public CommandPublisher(IPipelineFactory<IPipeline> pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.serializer = serializer;
        }

        protected override bool PublishInternal(ICommand message)
        {
            var endpointMessage = message.AsEndpointMessage(serializer);
            var commandType = message.GetType();
            endpointMessage.Headers.Add(MessageInfo.GetMessageId(commandType), String.Empty);
            pipelineFactory
                .GetPipeline(commandType)
                .Push(endpointMessage);
            return true;
        }
    }
}