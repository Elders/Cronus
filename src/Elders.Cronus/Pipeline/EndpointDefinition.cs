using System.Collections.Generic;

namespace Elders.Cronus.Pipeline
{
    public class EndpointDefinition
    {
        public string PipelineName { get; set; }

        public string EndpointName { get; private set; }

        public Dictionary<string, object> RoutingHeaders { get; private set; }

        public EndpointDefinition(string endpointName, Dictionary<string, object> routingHeaders, string pipelineName)
        {
            EndpointName = endpointName;
            PipelineName = pipelineName;
            RoutingHeaders = routingHeaders;
        }
    }
}