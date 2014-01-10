using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.Transports
{
    public class EndpointDefinition
    {
        public string PipelineName { get; set; }

        public string EndpointName { get; private set; }

        //public List<obj> HandledMessagesIds { get; private set; }
        public Dictionary<string, object> AcceptanceHeaders { get; private set; }
        public EndpointDefinition(string endpointName, Dictionary<string, object> acceptanceHeaders, string pipelineName)
        {
            EndpointName = endpointName;
            PipelineName = pipelineName;
            AcceptanceHeaders = acceptanceHeaders;
        }
    }
}
