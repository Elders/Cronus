using System;
using System.Collections.Generic;

namespace Elders.Cronus.Pipeline
{
    public class EndpointDefinition
    {
        public EndpointDefinition(string pipelineName, string endpointName, Dictionary<string, object> routingHeaders = null, string routingKey = "")
        {
            RoutingKey = routingKey;
            EndpointName = endpointName;
            PipelineName = pipelineName;
            RoutingHeaders = routingHeaders ?? new Dictionary<string, object>();
        }

        public string EndpointName { get; private set; }

        public bool IsRoutingDefined { get { return RoutingHeaders.Count != 0 || !String.IsNullOrEmpty(RoutingKey); } }

        public string PipelineName { get; set; }

        public Dictionary<string, object> RoutingHeaders { get; private set; }

        public string RoutingKey { get; set; }
    }
}