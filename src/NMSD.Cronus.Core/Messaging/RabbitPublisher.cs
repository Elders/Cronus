using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NSMD.Cronus.RabbitMQ;
using Protoreg;

namespace NMSD.Cronus.Core.Messaging
{
    public class RabbitPublisher<TMessage> : Publisher<TMessage>
    {
        string pipelineName;

        private readonly ProtoregSerializer serializer;

        public RabbitPublisher(ProtoregSerializer serializer)
        {
            this.serializer = serializer;
        }

        protected override bool PublishInternal(TMessage message)
        {
            pipelineName = GetPipelineName(message);
            var plumber = new Plumber();
            var pipe = plumber.GetPipeline(pipelineName);
            var buffer = SerializeMessage(message);

            DataContractAttribute contract = message.GetType().GetCustomAttributes(false).Where(attr => attr is DataContractAttribute).SingleOrDefault() as DataContractAttribute;
            pipe.KickIn(buffer, contract.Name);

            return true;
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
                    throw new Exception(String.Format(@"The message of type '{0}' has a different Namespace='{1}' than the expected Namespace='{2}'", message.GetType().FullName, pipelineNameFromContract, pipelineName));
            }

            return pipelineName;
        }

        private byte[] SerializeMessage(TMessage message)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, message);
                return stream.ToArray();
            }
        }
    }
}
