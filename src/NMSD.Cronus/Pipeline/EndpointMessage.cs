using System.Collections.Generic;

namespace NMSD.Cronus.Pipeline
{
    public class EndpointMessage
    {
        public EndpointMessage(byte[] body)
        {
            Body = body;
            Headers = new Dictionary<string, object>();
        }

        public EndpointMessage(byte[] body, IDictionary<string, object> headers)
        {
            Headers = headers;
            Body = body;
        }

        public byte[] Body { get; set; }

        public IDictionary<string, object> Headers { get; set; }

    }
}