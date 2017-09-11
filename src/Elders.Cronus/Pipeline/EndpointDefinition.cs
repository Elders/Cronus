using System.Collections.Generic;

namespace Elders.Cronus.Pipeline
{
    public class EndpointDefinition
    {
        public EndpointDefinition(string pipelineName, string endpointName, ICollection<string> watchMessageTypes = null)
        {
            EndpointName = endpointName;
            PipelineName = pipelineName;
            WatchMessageTypes = watchMessageTypes ?? new List<string>();
        }

        public string EndpointName { get; private set; }

        public bool IsRoutingDefined { get { return WatchMessageTypes.Count != 0; } }

        public string PipelineName { get; set; }

        public ICollection<string> WatchMessageTypes { get; set; }
    }
}