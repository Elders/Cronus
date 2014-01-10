using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.RabbitMQ;
using NMSD.Protoreg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections.Concurrent;
using NMSD.Cronus.Core.Transports;
namespace NMSD.Cronus.Core.Commanding
{
    public class CommandPublisher : IPublisher<ICommand>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CommandPublisher));

        private readonly ICommandPipelineConvention convetion;

        private readonly IPipelineFactory pipelineFactory;

        Dictionary<Type, IPipeline> pipelines = new Dictionary<Type, IPipeline>();

        private readonly ProtoregSerializer serializer;

        public CommandPublisher(ICommandPipelineConvention convetion, IPipelineFactory pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.convetion = convetion;
            this.serializer = serializer;

        }

        public bool Publish(ICommand command)
        {
            var endpointMessage = ToEndpointMessage(command);
            var commandType = command.GetType();
            endpointMessage.Headers.Add(MessagingHelper.GetMessageId(commandType), String.Empty);
            BuildPipeline(commandType);
            pipelines[commandType].Push(endpointMessage);
            log.Info("COMMAND SENT => " + command.ToString());
            return true;
        }

        private EndpointMessage ToEndpointMessage(ICommand cmd)
        {
            byte[] body;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, cmd);
                body = stream.ToArray();
            }
            return new EndpointMessage(body);
        }

        private void BuildPipeline(Type commandType)
        {
            if (!pipelines.ContainsKey(commandType))
            {
                var pipelineName = convetion.GetPipelineName(commandType);
                var pipeline = pipelineFactory.GetPipeline(pipelineName);
                pipelines[commandType] = pipeline;
            }
        }
    }

}