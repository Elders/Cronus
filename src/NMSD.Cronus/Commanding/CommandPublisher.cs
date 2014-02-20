using System;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports;
using NMSD.Protoreg;

namespace NMSD.Cronus.Commanding
{
    public class CommandPublisher : IPublisher<ICommand>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CommandPublisher));

        private readonly IPipelineFactory pipelineFactory;

        private readonly ProtoregSerializer serializer;

        public CommandPublisher(IPipelineFactory pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.serializer = serializer;
        }

        public bool Publish(ICommand command)
        {
            var endpointMessage = command.AsEndpointMessage(serializer);
            var commandType = command.GetType();
            endpointMessage.Headers.Add(MessageInfo.GetMessageId(commandType), String.Empty);
            pipelineFactory.GetPipeline(commandType).Push(endpointMessage);
            log.Info("COMMAND SENT => " + command.ToString());
            return true;
        }
    }
}