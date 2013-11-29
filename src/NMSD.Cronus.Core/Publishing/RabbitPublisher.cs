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

namespace NMSD.Cronus.Core.Publishing
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

            //  START not here
            IDictionary specs = new Dictionary<string, string>();
            specs.Add("x-match", "any");
            specs.Add("aeaae173-d790-443d-92b2-caa06d55f1a2", String.Empty);
            specs.Add("aeaae173-d790-443d-92b2-caa06d55f1a3", String.Empty);
            var endpoint = plumber.GetEndpoint(pipelineName + ".CommandsHeaders", specs);
            pipe.AttachEndpoint(endpoint);
            //  END not here

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

            DataContractAttribute contract = message.GetType().GetCustomAttributes(false).Where(attr => attr is DataContractAttribute).SingleOrDefault() as DataContractAttribute;

            if (contract == null)
                throw new Exception(String.Format(@"The message type '{0}' is missing a DataContract attribute. Example: [DataContract(Name = ""00000000-0000-0000-0000-000000000000"", Namespace = ""Company.Product.BoundedContext"")]", message.GetType().FullName));

            if (String.IsNullOrWhiteSpace(contract.Namespace))
                throw new Exception(String.Format(@"The message type '{0}' is missing a DataContract attribute property Namespace. Example: [DataContract(Name = ""00000000-0000-0000-0000-000000000000"", Namespace = ""Company.Product.BoundedContext"")]", message.GetType().FullName));

            string pipelineNameFromContract;
            try { pipelineNameFromContract = contract.Namespace.Substring(0, contract.Namespace.LastIndexOf('.')); }
            catch (ArgumentOutOfRangeException) { throw new Exception(String.Format(@"The message type '{0}' has invalid DataContract attribute property Namespace='{1}'. Example: [DataContract(Name = ""00000000-0000-0000-0000-000000000000"", Namespace = ""Company.Product.BoundedContext"")]", message.GetType().FullName, contract.Namespace)); }

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
