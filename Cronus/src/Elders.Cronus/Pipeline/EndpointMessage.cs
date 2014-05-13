using System.Collections.Generic;

namespace Elders.Cronus.Pipeline
{
    public class EndpointMessage
    {
        public EndpointMessage(byte[] body, string routingKey = "", IDictionary<string, object> routingHeaders = null)
        {
            RoutingKey = routingKey;
            RoutingHeaders = routingHeaders ?? new Dictionary<string, object>();
            Body = body;
        }

        public byte[] Body { get; set; }

        public IDictionary<string, object> RoutingHeaders { get; set; }

        public string RoutingKey { get; set; }

    }
}