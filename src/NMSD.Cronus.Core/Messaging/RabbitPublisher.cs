using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NSMD.Cronus.RabbitMQ;
using Protoreg;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;

namespace NMSD.Cronus.Core.Messaging
{
    public class RabbitPublisher<TMessage> : Publisher<TMessage> where TMessage : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RabbitPublisher<TMessage>));

        string pipelineName;

        private readonly ProtoregSerializer serializer;

        IConnection connection;
        IModel channel;

        public RabbitPublisher(ProtoregSerializer serializer)
        {
            this.serializer = serializer;
            BoundedContext = String.Empty;
            connection = new Plumber().RabbitConnection;
            channel = connection.CreateModel();
        }

        public string BoundedContext { get; protected set; }

        Dictionary<Type, string> messageIds = new Dictionary<Type, string>();

        protected override bool PublishInternal(TMessage message)
        {
            BuildPipeline(message);

            string messageId = null;
            Type messageType = message.GetType();
            if (!messageIds.TryGetValue(messageType, out messageId))
            {
                DataContractAttribute contract = messageType.GetCustomAttributes(false).Where(attr => attr is DataContractAttribute).SingleOrDefault() as DataContractAttribute;
                messageIds[messageType] = contract.Name;
                messageId = contract.Name;
            }

            var buffer = SerializeMessage(message);
            KickIn(buffer, messageId, BoundedContext);
            log.Info("PUBLISH => " + message.ToString());

            return true;
        }

        public void KickIn(byte[] message, string messageId, string boundedContext = "")
        {
            var properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object>();
            properties.Headers.Add(messageId, String.Empty);
            properties.Headers.Add(boundedContext, String.Empty);
            properties.SetPersistent(true);
            properties.Priority = 9;
            channel.BasicPublish(pipelineName, String.Empty, false, false, properties, message);
        }

        protected void BuildPipeline(TMessage message)
        {
            if (typeof(TMessage) == typeof(NMSD.Cronus.Core.Cqrs.MessageCommit))
                pipelineName = pipelineName ?? "NMSD.Cronus.Sample.EventStore";
            else
                pipelineName = pipelineName ?? GetPipelineName(message);
        }

        string GetPipelineName(TMessage message)
        {
            bool forceDataContractCheck = false;
#if DEBUG
            forceDataContractCheck = true;
#endif
            if (!forceDataContractCheck && !String.IsNullOrEmpty(pipelineName))
                return pipelineName;

            string pipelineNameFromContract = MessagingHelper.GetPipelineName(message);
            if (String.IsNullOrEmpty(pipelineName))
            {
                pipelineName = pipelineNameFromContract;
            }
            else
            {
                if (pipelineName != pipelineNameFromContract)
                    throw new Exception(String.Format(@"The message of type '{0}' has a different BoundedContextNamespace='{1}' than the expected BoundedContextNamespace='{2}'", message.GetType().FullName, pipelineNameFromContract, pipelineName));
            }

            return pipelineName;
        }

        protected byte[] SerializeMessage(TMessage message)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, message);
                return stream.ToArray();
            }
        }
    }
}