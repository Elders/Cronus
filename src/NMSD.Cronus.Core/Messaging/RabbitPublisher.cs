using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NMSD.Cronus.Core.Commanding;
using NSMD.Cronus.RabbitMQ;
using Protoreg;

namespace NMSD.Cronus.Core.Messaging
{
    public class RabbitPublisher<TMessage> : Publisher<TMessage> where TMessage : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RabbitPublisher<TMessage>));

        string pipelineName;

        private readonly ProtoregSerializer serializer;

        public RabbitPublisher(ProtoregSerializer serializer)
        {
            this.serializer = serializer;
        }

        protected override bool PublishInternal(TMessage message)
        {
            BuildPipeline(message);
            var buffer = SerializeMessage(message);

            DataContractAttribute contract = message.GetType().GetCustomAttributes(false).Where(attr => attr is DataContractAttribute).SingleOrDefault() as DataContractAttribute;
            pipe.KickIn(buffer, contract.Name);
            log.Info("PUBLISH => " + message.ToString());

            return true;
        }

        Pipeline pipe;

        protected void BuildPipeline(TMessage message)
        {
            if (pipe == null)
            {
                pipelineName = GetPipelineName(message);
                var plumber = new Plumber("192.168.16.69");
                pipe = plumber.GetPipeline(pipelineName);
            }
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
