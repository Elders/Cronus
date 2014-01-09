using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.Core.Transports
{
    public class EndpointDefinition
    {
        public string PipelineName { get; set; }

        public string EndpointName { get; private set; }

        public List<Guid> HandledMessagesIds { get; private set; }

        public EndpointDefinition(string endpointName, List<Guid> endpointMessagesDefinitions, string pipelineName)
        {
            EndpointName = endpointName;
            HandledMessagesIds = endpointMessagesDefinitions;
            PipelineName = pipelineName;
        }
    }
}
